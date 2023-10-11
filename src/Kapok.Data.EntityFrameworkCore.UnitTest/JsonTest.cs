using System.Text.Json.Nodes;
using Kapok.BusinessLayer;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Kapok.Module;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

/// <summary>
/// Testing of json properties in classes. 
/// </summary>
public class JsonTest : DeferredDaoTestBase
{
    protected override void InitiateModule()
    {
        ModuleEngine.InitiateModule(typeof(SampleModelModule));
    }

    [Fact]
    public async Task JsonObjectPropertyTest()
    {
        {
            using var scope = DataDomain.CreateScope();
            var courseDao = scope.GetDao<Course, DeferredDao<Course>>();

            var newCourse = courseDao.New();
            newCourse.CourseId = 1;
            newCourse.Title = "Leaning about JSON and its benefits compared to other formats";
            newCourse.Metadata = (JsonObject?)JsonNode.Parse(@"{""Language"": ""en-US""}");
            Assert.NotNull(newCourse.Metadata);
            Assert.Equal("en-US", newCourse.Metadata?["Language"]?.GetValue<string>());
            await courseDao.CreateAsync(newCourse);
            await scope.SaveAsync();
        }

        {
            using var scope = DataDomain.CreateScope();
            var courseDao = scope.GetDao<Course, DeferredDao<Course>>();

            var course = courseDao.GetByKey(1);
            Assert.Equal(1, course.CourseId);
            Assert.Equal("Leaning about JSON and its benefits compared to other formats", course.Title);
            Assert.NotNull(course.Metadata);
            Assert.Equal("en-US", course.Metadata?["Language"]?.GetValue<string>());
        }
    }
}