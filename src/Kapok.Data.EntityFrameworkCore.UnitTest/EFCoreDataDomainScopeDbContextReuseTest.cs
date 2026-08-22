using Kapok.BusinessLayer;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Kapok.Module;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

/// <summary>
/// Verifies that EFCoreDataDomainScope reuses its DbContext instance across multiple
/// Save() calls within the same scope instead of disposing and lazily reconstructing
/// a brand new one on every Save().
/// </summary>
public class EFCoreDataDomainScopeDbContextReuseTest : DeferredEntityServiceTestBase
{
    protected override void InitiateModule()
    {
        Kapok.Data.DataDomain.DefaultEntityServiceType = typeof(EntityDeferredCommitService<>);
        ModuleEngine.InitiateModule(typeof(SampleModelModule));
    }

    [Fact]
    public async Task Save_ReusesTheSameDbContext_AcrossMultipleSavesInOneScope()
    {
        using var scope = (EFCoreDataDomainScope)DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        var course = courseService.New();
        course.Title = "Reuse test course";
        course.Credits = 5;
        await courseService.CreateAsync(course);

        await scope.SaveAsync();
        var dbContextAfterFirstSave = scope.DbContext;
        Assert.NotNull(dbContextAfterFirstSave);

        var courseFromDb = courseService.AsQueryableForUpdate().First(c => c.Title == "Reuse test course");
        courseFromDb.Credits = 6;
        await scope.SaveAsync();
        var dbContextAfterSecondSave = scope.DbContext;

        // the core assertion this PR is about: the same DbContext instance is reused,
        // it is not disposed and lazily reconstructed on every Save()
        Assert.Same(dbContextAfterFirstSave, dbContextAfterSecondSave);

        // and the change is still correctly persisted/readable
        Assert.Equal(6, courseService.AsQueryable().First(c => c.Title == "Reuse test course").Credits);
    }

    [Fact]
    public async Task RejectChanges_KeepsTheDbContextAlive_ForReuseInTheSameScope()
    {
        using var scope = (EFCoreDataDomainScope)DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        var course = courseService.New();
        course.Title = "Reject test course";
        course.Credits = 5;
        await courseService.CreateAsync(course);

        var dbContextBeforeReject = scope.DbContext;
        Assert.NotNull(dbContextBeforeReject);

        scope.RejectChanges();

        var dbContextAfterReject = scope.DbContext;
        Assert.Same(dbContextBeforeReject, dbContextAfterReject);

        Assert.False(scope.CanSave());
        Assert.Empty(courseService.AsQueryable().Where(c => c.Title == "Reject test course"));
    }
}
