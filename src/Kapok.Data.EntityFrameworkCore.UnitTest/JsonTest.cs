using System.Text.Json.Nodes;
using Kapok.BusinessLayer;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Kapok.Module;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

/// <summary>
/// Testing of json properties in classes. 
/// </summary>
public class JsonTest : DeferredEntityServiceTestBase
{
    protected override void InitiateModule()
    {
        Data.DataDomain.DefaultEntityServiceType = typeof(EntityDeferredCommitService<>);
        ModuleEngine.InitiateModule(typeof(SampleModelModule));
    }

    [Fact]
    public async Task JsonObjectPropertyTest()
    {
        {
            using var scope = DataDomain.CreateScope();
            var courseService = scope.GetEntityService<Course, EntityDeferredCommitService<Course>>();

            var newCourse = courseService.New();
            newCourse.CourseId = 1;
            newCourse.Title = "Leaning about JSON and its benefits compared to other formats";
            newCourse.Metadata = (JsonObject?)JsonNode.Parse(@"{""Language"": ""en-US""}");
            Assert.NotNull(newCourse.Metadata);
            Assert.Equal("en-US", newCourse.Metadata?["Language"]?.GetValue<string>());
            await courseService.CreateAsync(newCourse);
            await scope.SaveAsync();
        }

        {
            using var scope = DataDomain.CreateScope();
            var courseService = scope.GetEntityService<Course, EntityDeferredCommitService<Course>>();

            var course = courseService.GetByKey(1);
            Assert.Equal(1, course.CourseId);
            Assert.Equal("Leaning about JSON and its benefits compared to other formats", course.Title);
            Assert.NotNull(course.Metadata);
            Assert.Equal("en-US", course.Metadata?["Language"]?.GetValue<string>());
        }
    }
}