using System.Diagnostics;
using Kapok.BusinessLayer;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Kapok.Module;
using Microsoft.EntityFrameworkCore;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

public class DeferredEntityServiceSampleModelTests : DeferredEntityServiceTestBase
{
    protected override void InitiateModule()
    {
        Kapok.Data.DataDomain.DefaultEntityServiceType = typeof(EntityDeferredCommitService<>);
        ModuleEngine.InitiateModule(typeof(SampleModelModule));
    }

    private static async Task SeedMathCourse(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        var newCourse = courseService.New();
        newCourse.Title = $"Math course {DateTime.Now.Year}";
        newCourse.Credits = 15;
        await courseService.CreateAsync(newCourse);

        await scope.SaveAsync();

        Assert.Equal(1, courseService.AsQueryable().Count());
    }

    private static async Task SeedJohnAdamsStudent(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var studentService = scope.GetEntityService<Student>();

        var johnAdams = studentService.New();
        johnAdams.FirstMidName = "John";
        johnAdams.LastName = "Adams";
        johnAdams.EnrollmentDate = DateTime.Now.Date;
        await studentService.CreateAsync(johnAdams);

        await scope.SaveAsync();
    }

    private static async Task SeedJohnAdamsEnrollment(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var enrollmentService = scope.GetEntityService<Enrollment>();

        var newEnrollment = enrollmentService.New();
        newEnrollment.CourseId = scope.GetEntityService<Course>().AsQueryable()
            .First(c => c.Title.StartsWith("Math course")).CourseId;
        newEnrollment.StudentId = scope.GetEntityService<Student>().AsQueryable()
            .First(s => s.FirstMidName == "John" && s.LastName == "Adams").Id;
        newEnrollment.Grade = Grade.F;
        await enrollmentService.CreateAsync(newEnrollment);

        await scope.SaveAsync();
    }

    [Fact]
    public void CheckEmptyDatabase()
    {
        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course, EntityDeferredCommitService<Course>>();
        var enrollmentService = scope.GetEntityService<Enrollment, EntityDeferredCommitService<Enrollment>>();
        var studentService = scope.GetEntityService<Student, EntityDeferredCommitService<Student>>();

        Assert.False(courseService.CanSave());
        Assert.False(enrollmentService.CanSave());
        Assert.False(studentService.CanSave());

        Assert.Empty(courseService.AsQueryable());
        Assert.Empty(enrollmentService.AsQueryable());
        Assert.Empty(studentService.AsQueryable());
    }

    /// <summary>
    /// Tests if the database seeding works as expected
    /// </summary>
    [Fact]
    public async void SeedDatabaseTest()
    {
        await SeedMathCourse(DataDomain);
        await SeedJohnAdamsStudent(DataDomain);
        await SeedJohnAdamsEnrollment(DataDomain);

        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        Assert.Equal(1, courseService.AsQueryable().Count());

        var mathCourse = courseService.AsQueryable().FirstOrDefault();
        Assert.NotNull(mathCourse);
        Debug.Assert(mathCourse != null);
        Assert.NotEqual(default, mathCourse.CourseId);
        Assert.Equal(15, mathCourse.Credits);
        Assert.StartsWith("Math course", mathCourse.Title);


        var studentService = scope.GetEntityService<Student>();

        Assert.Equal(1, studentService.AsQueryable().Count());

        var johnAdams = studentService.AsQueryable().FirstOrDefault();
        Assert.NotNull(johnAdams);
        Debug.Assert(johnAdams != null);
        Assert.NotEqual(default, johnAdams.Id);
        Assert.Equal("John", johnAdams.FirstMidName);
        Assert.Equal("Adams", johnAdams.LastName);
        Assert.Equal(DateTime.Now.Date, johnAdams.EnrollmentDate);


        var enrollmentService = scope.GetEntityService<Enrollment>();

        Assert.Equal(1, enrollmentService.AsQueryable().Count());

        var johnAdamsMathCourseEnrollment = enrollmentService.AsQueryable().FirstOrDefault();
        Assert.NotNull(johnAdamsMathCourseEnrollment);
        Debug.Assert(johnAdamsMathCourseEnrollment != null);
        Assert.NotEqual(default, johnAdamsMathCourseEnrollment.EnrollmentId); 
        Assert.NotEqual(default, johnAdamsMathCourseEnrollment.CourseId);
        Assert.NotEqual(default, johnAdamsMathCourseEnrollment.StudentId);
        Assert.Equal(Grade.F, johnAdamsMathCourseEnrollment.Grade);
    }

    [Fact]
    public async Task DeleteSeededDataTest()
    {
        await SeedMathCourse(DataDomain);
        await SeedJohnAdamsStudent(DataDomain);
        await SeedJohnAdamsEnrollment(DataDomain);
        // ... todo add other seeded data

        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course, EntityDeferredCommitService<Course>>();
        await courseService.DeleteRangeAsync(courseService.AsQueryableForUpdate());
        var enrollmentService = scope.GetEntityService<Enrollment, EntityDeferredCommitService<Enrollment>>();
        await enrollmentService.DeleteRangeAsync(enrollmentService.AsQueryableForUpdate());
        var studentService = scope.GetEntityService<Student, EntityDeferredCommitService<Student>>();
        await studentService.DeleteRangeAsync(studentService.AsQueryableForUpdate());

        Assert.NotEmpty(courseService.AsQueryable());
        Assert.NotEmpty(enrollmentService.AsQueryable());
        Assert.NotEmpty(studentService.AsQueryable());

        await scope.SaveAsync();

        Assert.Empty(courseService.AsQueryable());
        Assert.Empty(enrollmentService.AsQueryable());
        Assert.Empty(studentService.AsQueryable());
    }

    // NOTE: this unit test should be moved to a separate EntityDeferredCommitService<> unit test class
    [Fact]
    public async Task CreateAndUpdateChangeTrackingTest()
    {
        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        // note: from SeedMathCourse(...)
        var newCourse = courseService.New();
        newCourse.Title = $"Math course {DateTime.Now.Year}";
        newCourse.Credits = 15;
        await courseService.CreateAsync(newCourse);

        Assert.Empty(courseService.AsQueryable());

        await courseService.UpdateAsync(newCourse);

        Assert.Empty(courseService.AsQueryable());

        await scope.SaveAsync();

        Assert.Equal(1, courseService.AsQueryable().Count());
    }

    [Fact]
    public async Task ChangeTrackingAndUpdateTest()
    {
        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        await SeedMathCourse(DataDomain);

        Assert.Equal(1, courseService.AsQueryable().Count());
        Assert.Equal(1, await courseService.AsQueryable().CountAsync());

        Assert.Equal(15, courseService.AsQueryable().First().Credits);

        // test update via 'StartChangeTracking'
        using (var scope2 = DataDomain.CreateScope())
        {
            var courseService2 = scope2.GetEntityService<Course>();

            var courseFromDb = courseService2.AsQueryable().First();
            ((EntityDeferredCommitService<Course>)courseService2).StartChangeTracking(courseFromDb);

            Assert.Equal(15, courseFromDb.Credits);
            courseFromDb.Credits = 20;

            Assert.Equal(15, courseService2.AsQueryable().First().Credits);

            await scope2.SaveAsync();
            Assert.Equal(20, courseService2.AsQueryable().First().Credits);
        }

        // test update via 'AsQueryableForUpdate'
        using (var scope2 = DataDomain.CreateScope())
        {
            var courseService2 = scope2.GetEntityService<Course>();

            var courseFromDb = courseService2.AsQueryableForUpdate().First();

            Assert.Equal(20, courseFromDb.Credits);
            courseFromDb.Credits = 25;

            Assert.Equal(20, courseService2.AsQueryable().First().Credits);

            await scope2.SaveAsync();
            Assert.Equal(25, courseService2.AsQueryable().First().Credits);
        }

        // reset change
        var courseFromService = courseService.AsQueryableForUpdate().First();
        Assert.Equal(25, courseFromService.Credits);
        courseFromService.Credits = 15;
        await scope.SaveAsync();
        Assert.Equal(15, courseService.AsQueryable().First().Credits);

        // test update via 'Update'
        /*
        using (var scope2 = DataDomain.CreateScope())
        {
            var courseService2 = scope2.GetEntityService<Course>();

            var courseFromDb = courseService2.AsQueryable().First();

            Assert.Equal(15, courseFromDb.Credits);
            courseFromDb.Credits = 20;
            await courseService2.UpdateAsync(courseFromDb);

            Assert.Equal(15, courseService2.AsQueryable().First().Credits);

            await scope2.SaveAsync();
            Assert.Equal(20, courseService2.AsQueryable().First().Credits);
        }*/

        // reset change
        courseService.AsQueryableForUpdate().First().Credits = 15;
        await scope.SaveAsync();

        // test no update via 'AsQueryable'
        using (var scope2 = DataDomain.CreateScope())
        {
            var courseService2 = scope2.GetEntityService<Course>();

            var courseFromDb = courseService2.AsQueryable().First();

            Assert.Equal(15, courseFromDb.Credits);
            courseFromDb.Credits = 20;

            Assert.Equal(15, courseService2.AsQueryable().First().Credits);

            await scope2.SaveAsync();
            Assert.Equal(15, courseService2.AsQueryable().First().Credits);
        }

        // test no update via 'NotForUpdate'
        using (var scope2 = DataDomain.CreateScope())
        {
            var courseService2 = scope2.GetEntityService<Course>();

            var courseFromDb = courseService2.AsQueryableForUpdate().NotForUpdate().First();

            Assert.Equal(15, courseFromDb.Credits);
            courseFromDb.Credits = 20;

            Assert.Equal(15, courseService2.AsQueryable().First().Credits);

            await scope2.SaveAsync();
            Assert.Equal(15, courseService2.AsQueryable().First().Credits);
        }
    }

    [Fact]
    public async Task UpdateNotTrackedEntityFailTest()
    {
        await SeedMathCourse(DataDomain);

        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        var mathCourse = courseService.AsQueryable().First();
        mathCourse.Credits = 20;

        Assert.Throws<NotSupportedException>(() => courseService.Update(mathCourse));
    }

    [Fact(Skip = "SQLite does not support RowVersion/Timestamp")]
    public async Task TestUpdateRowVersion()
    {
        await SeedMathCourse(DataDomain);

        using var scope = DataDomain.CreateScope();
        var courseService = scope.GetEntityService<Course>();

        var mathCourse = courseService.AsQueryableForUpdate().First();
        Assert.NotNull(mathCourse);
        Debug.Assert(mathCourse != null);

        Assert.NotNull(mathCourse.RowVersion);
        Debug.Assert(mathCourse.RowVersion != null);
        Assert.NotEmpty(mathCourse.RowVersion);

        byte[] oldRowVersion = new byte[mathCourse.RowVersion.Length];
        mathCourse.RowVersion.CopyTo(oldRowVersion, 0);

        mathCourse.Credits = 20;
        await courseService.UpdateAsync(mathCourse);

        // the DB was not updated yet so the RowVersion column still should have the same values
        Assert.True(mathCourse.RowVersion.SequenceEqual(oldRowVersion));

        await scope.SaveAsync();

        // the DB was now updated, so the RowVersion column should have been updated from the database
        Assert.False(mathCourse.RowVersion.SequenceEqual(oldRowVersion));
    }
}