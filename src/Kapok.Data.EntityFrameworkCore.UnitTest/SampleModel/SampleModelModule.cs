using Kapok.Core;

namespace Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;

/// <summary>
/// A module providing a sample data model based on the following example:
///
/// https://docs.microsoft.com/de-de/aspnet/core/data/ef-mvc/intro?view=aspnetcore-5.0
/// </summary>
public sealed class SampleModelModule : ModuleBase
{
    public SampleModelModule()
        : base(nameof(SampleModelModule))
    {
        AddDependsOnModule(typeof(Kapok.Core.CoreModule));
    }

    public override void Initiate()
    {
        base.Initiate();

        // register entities
        DataDomain.RegisterEntity<Course>();
        DataDomain.RegisterEntity<Enrollment>();
        DataDomain.RegisterEntity<Student>();
    }
}