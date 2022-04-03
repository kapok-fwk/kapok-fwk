using System;
using System.Linq;
using Kapok.Core;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kapok.Data.EntityFrameworkCore.UnitTest
{
    public class DeferredDaoSampleModelTests : IDisposable
    {
        private readonly IDataDomain _dataDomain;

        public DeferredDaoSampleModelTests()
        {
            DataDomain.DefaultDaoType = typeof(DeferredDao<>);
            _dataDomain = InitializeDataDomain();
        }

        public void Dispose()
        {
        }

        public static IDataDomain InitializeDataDomain()
        {
            ModuleEngine.InitiateModule(typeof(SampleModelModule));

            var contextOptionBuilder = new DbContextOptionsBuilder();
            contextOptionBuilder.UseSqlServer(@"Server=(local);Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0",
                opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
            var dataDomain = new EFCoreDataDomain(contextOptionBuilder.Options);

            // drop and recreate test database
            using var dbContext = new DbContextBase(dataDomain.DbContextOptions);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

            return dataDomain;
        }

        private static async void SeedMathCourse(IDataDomain dataDomain)
        {
            using var scope = dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();

            var newCourse = courseDao.New();
            newCourse.Title = $"Math course {DateTime.Now.Year}";
            newCourse.Credits = 15;
            await courseDao.CreateAsync(newCourse);

            await scope.SaveAsync();
        }

        private static async void SeedJohnAdamsStudent(IDataDomain dataDomain)
        {
            using var scope = dataDomain.CreateScope();
            var studentDao = scope.GetDao<Student>();

            var johnAdams = studentDao.New();
            johnAdams.FirstMidName = "John";
            johnAdams.LastName = "Adams";
            johnAdams.EnrollmentDate = DateTime.Now.Date;
            await studentDao.CreateAsync(johnAdams);

            await scope.SaveAsync();
        }

        private static async void SeedJohnAdamsEnrollment(IDataDomain dataDomain)
        {
            using var scope = dataDomain.CreateScope();
            var enrollmentDao = scope.GetDao<Enrollment>();

            var newEnrollment = enrollmentDao.New();
            newEnrollment.CourseId = scope.GetDao<Course>().AsQueryable()
                .First(c => c.Title.StartsWith("Math course")).CourseId;
            newEnrollment.StudentId = scope.GetDao<Student>().AsQueryable()
                .First(s => s.FirstMidName == "John" && s.LastName == "Adams").Id;
            newEnrollment.Grade = Grade.F;
            await enrollmentDao.CreateAsync(newEnrollment);

            await scope.SaveAsync();
        }

        [Fact]
        public void CheckEmptyDatabase()
        {
            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course, DeferredDao<Course>>();
            var enrollmentDao = scope.GetDao<Enrollment, DeferredDao<Enrollment>>();
            var studentDao = scope.GetDao<Student, DeferredDao<Student>>();
            
            Assert.False(courseDao.CanSave());
            Assert.False(enrollmentDao.CanSave());
            Assert.False(studentDao.CanSave());

            Assert.Empty(courseDao.AsQueryable());
            Assert.Empty(enrollmentDao.AsQueryable());
            Assert.Empty(studentDao.AsQueryable());
        }

        /// <summary>
        /// Tests if the database seeding works as expected
        /// </summary>
        [Fact]
        public void SeedDatabaseTest()
        {
            SeedMathCourse(_dataDomain);
            SeedJohnAdamsStudent(_dataDomain);
            SeedJohnAdamsEnrollment(_dataDomain);

            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();


            Assert.Equal(1, courseDao.AsQueryable().Count());

            var mathCourse = courseDao.AsQueryable().FirstOrDefault();
            Assert.NotNull(mathCourse);
            Assert.NotEqual(default(int), mathCourse.CourseId);
            Assert.Equal(15, mathCourse.Credits);
            Assert.StartsWith("Math course", mathCourse.Title);


            var studentDao = scope.GetDao<Student>();

            Assert.Equal(1, studentDao.AsQueryable().Count());

            var johnAdams = studentDao.AsQueryable().FirstOrDefault();
            Assert.NotNull(johnAdams);
            Assert.NotEqual(default(int), johnAdams.Id);
            Assert.Equal("John", johnAdams.FirstMidName);
            Assert.Equal("Adams", johnAdams.LastName);
            Assert.Equal(DateTime.Now.Date, johnAdams.EnrollmentDate);


            var enrollmentDao = scope.GetDao<Enrollment>();

            Assert.Equal(1, enrollmentDao.AsQueryable().Count());

            var johnAdamsMathCourseEnrollment = enrollmentDao.AsQueryable().FirstOrDefault();
            Assert.NotNull(johnAdamsMathCourseEnrollment);
            Assert.NotEqual(default(int), johnAdamsMathCourseEnrollment.EnrollmentId); 
            Assert.NotEqual(default(int), johnAdamsMathCourseEnrollment.CourseId);
            Assert.NotEqual(default(int), johnAdamsMathCourseEnrollment.StudentId);
            Assert.Equal(Grade.F, johnAdamsMathCourseEnrollment.Grade);
        }

        [Fact]
        public async void DeleteSeededDataTest()
        {
            SeedMathCourse(_dataDomain);
            SeedJohnAdamsStudent(_dataDomain);
            SeedJohnAdamsEnrollment(_dataDomain);
            // ... todo add other seeded data

            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course, DeferredDao<Course>>();
            await courseDao.DeleteRangeAsync(courseDao.AsQueryableForUpdate());
            var enrollmentDao = scope.GetDao<Enrollment, DeferredDao<Enrollment>>();
            await enrollmentDao.DeleteRangeAsync(enrollmentDao.AsQueryableForUpdate());
            var studentDao = scope.GetDao<Student, DeferredDao<Student>>();
            await studentDao.DeleteRangeAsync(studentDao.AsQueryableForUpdate());

            Assert.NotEmpty(courseDao.AsQueryable());
            Assert.NotEmpty(enrollmentDao.AsQueryable());
            Assert.NotEmpty(studentDao.AsQueryable());

            await scope.SaveAsync();

            Assert.Empty(courseDao.AsQueryable());
            Assert.Empty(enrollmentDao.AsQueryable());
            Assert.Empty(studentDao.AsQueryable());
        }

        // NOTE: this unit test should be moved to a separate DeferredDao<> unit test class
        [Fact]
        public async void CreateAndUpdateChangeTrackingTest()
        {
            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();

            // note: from SeedMathCourse(...)
            var newCourse = courseDao.New();
            newCourse.Title = $"Math course {DateTime.Now.Year}";
            newCourse.Credits = 15;
            await courseDao.CreateAsync(newCourse);

            Assert.Empty(courseDao.AsQueryable());

            await courseDao.UpdateAsync(newCourse);

            Assert.Empty(courseDao.AsQueryable());

            await scope.SaveAsync();

            Assert.Equal(1, courseDao.AsQueryable().Count());
        }

        [Fact]
        public async void ChangeTrackingAndUpdateTest()
        {
            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();

            SeedMathCourse(_dataDomain);

            Assert.Equal(1, courseDao.AsQueryable().Count());
            Assert.Equal(1, await courseDao.AsQueryable().CountAsync());

            Assert.Equal(15, courseDao.AsQueryable().First().Credits);

            // test update via 'StartChangeTracking'
            using (var scope2 = _dataDomain.CreateScope())
            {
                var courseDao2 = scope2.GetDao<Course>();

                var courseFromDb = courseDao2.AsQueryable().First();
                ((DeferredDao<Course>)courseDao2).StartChangeTracking(courseFromDb);

                Assert.Equal(15, courseFromDb.Credits);
                courseFromDb.Credits = 20;

                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);

                await scope2.SaveAsync();
                Assert.Equal(20, courseDao2.AsQueryable().First().Credits);
            }

            // test update via 'AsQueryableForUpdate'
            using (var scope2 = _dataDomain.CreateScope())
            {
                var courseDao2 = scope2.GetDao<Course>();

                var courseFromDb = courseDao2.AsQueryableForUpdate().First();

                Assert.Equal(20, courseFromDb.Credits);
                courseFromDb.Credits = 25;

                Assert.Equal(20, courseDao2.AsQueryable().First().Credits);

                await scope2.SaveAsync();
                Assert.Equal(25, courseDao2.AsQueryable().First().Credits);
            }

            // reset change
            var courseFromDao = courseDao.AsQueryableForUpdate().First();
            Assert.Equal(25, courseFromDao.Credits);
            courseFromDao.Credits = 15;
            await scope.SaveAsync();
            Assert.Equal(15, courseDao.AsQueryable().First().Credits);

            // test update via 'Update'
            /*
            using (var scope2 = _dataDomain.CreateScope())
            {
                var courseDao2 = scope2.GetDao<Course>();

                var courseFromDb = courseDao2.AsQueryable().First();

                Assert.Equal(15, courseFromDb.Credits);
                courseFromDb.Credits = 20;
                await courseDao2.UpdateAsync(courseFromDb);

                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);

                await scope2.SaveAsync();
                Assert.Equal(20, courseDao2.AsQueryable().First().Credits);
            }*/

            // reset change
            courseDao.AsQueryableForUpdate().First().Credits = 15;
            await scope.SaveAsync();

            // test no update via 'AsQueryable'
            using (var scope2 = _dataDomain.CreateScope())
            {
                var courseDao2 = scope2.GetDao<Course>();

                var courseFromDb = courseDao2.AsQueryable().First();

                Assert.Equal(15, courseFromDb.Credits);
                courseFromDb.Credits = 20;

                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);

                await scope2.SaveAsync();
                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);
            }

            // test no update via 'NotForUpdate'
            using (var scope2 = _dataDomain.CreateScope())
            {
                var courseDao2 = scope2.GetDao<Course>();

                var courseFromDb = courseDao2.AsQueryableForUpdate().NotForUpdate().First();

                Assert.Equal(15, courseFromDb.Credits);
                courseFromDb.Credits = 20;

                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);

                await scope2.SaveAsync();
                Assert.Equal(15, courseDao2.AsQueryable().First().Credits);
            }
        }

        [Fact]
        public void UpdateNotTrackedEntityFailTest()
        {
            SeedMathCourse(_dataDomain);

            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();

            var mathCourse = courseDao.AsQueryable().First();
            mathCourse.Credits = 20;

            Assert.Throws<NotSupportedException>(() => courseDao.Update(mathCourse));
        }

        [Fact]
        public async void TestUpdateRowVersion()
        {
            SeedMathCourse(_dataDomain);

            using var scope = _dataDomain.CreateScope();
            var courseDao = scope.GetDao<Course>();

            var mathCourse = courseDao.AsQueryableForUpdate().First();
            Assert.NotNull(mathCourse);

            Assert.NotNull(mathCourse.RowVersion);
            Assert.NotEmpty(mathCourse.RowVersion);

            byte[] oldRowVersion = new byte[mathCourse.RowVersion.Length];
            mathCourse.RowVersion.CopyTo(oldRowVersion, 0);

            mathCourse.Credits = 20;
            await courseDao.UpdateAsync(mathCourse);

            // the DB was not updated yet so the RowVersion column still should have the same values
            Assert.True(mathCourse.RowVersion.SequenceEqual(oldRowVersion));

            await scope.SaveAsync();

            // the DB was now updated, so the RowVersion column should have been updated from the database
            Assert.False(mathCourse.RowVersion.SequenceEqual(oldRowVersion));
        }
    }
}
