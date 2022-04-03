using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Core;
using Kapok.Entity;

namespace Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel
{
    // model source: https://docs.microsoft.com/de-de/aspnet/core/data/ef-mvc/intro?view=aspnetcore-5.0

    public class Course : EditableEntityBase
    {
        static Course()
        {
            RegisterModel<Course>(e =>
            {
                e.SetPrimaryKey(nameof(CourseId));
            });
        }

        private byte[] _rowVersion;
        private int _courseId;
        private string _title;
        private int _credits;

        [Timestamp]
        [Browsable(false)]
        // ReSharper disable once UnusedMember.Global
        public byte[] RowVersion
        {
            get => _rowVersion;
            set => SetProperty(ref _rowVersion, value);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId
        {
            get => _courseId;
            set => SetProperty(ref _courseId, value);
        }

        public string Title
        {
            get => _title;
            set => SetValidateProperty(ref _title, value);
        }

        public int Credits
        {
            get => _credits;
            set => SetValidateProperty(ref _credits, value);
        }

        public ICollection<Enrollment> Enrollments { get; set; }
    }

    public enum Grade
    {
        A,
        B,
        C,
        D,
        F
    }

    public class Enrollment : EditableEntityBase
    {
        static Enrollment()
        {
            RegisterModel<Enrollment>(e =>
            {
                e.SetPrimaryKey(nameof(EnrollmentId));

                e.AddOneToManyRelationship<Enrollment>()
                    .HasForeignKey(nameof(EnrollmentId));
                e.AddOneToManyRelationship<Course>(nameof(Course))
                    .HasForeignKey(nameof(CourseId));
                e.AddOneToManyRelationship<Student>(nameof(Student))
                    .HasForeignKey(nameof(StudentId));
            });
        }

        private byte[] _rowVersion;
        private int _enrollmentId;
        private int _courseId;
        private int _studentId;
        private Grade? _grade;

        [Timestamp]
        [Browsable(false)]
        // ReSharper disable once UnusedMember.Global
        public byte[] RowVersion
        {
            get => _rowVersion;
            set => SetProperty(ref _rowVersion, value);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrollmentId
        {
            get => _enrollmentId;
            set => SetValidateProperty(ref _enrollmentId, value);
        }

        public int CourseId
        {
            get => _courseId;
            set => SetValidateProperty(ref _courseId, value);
        }

        public int StudentId
        {
            get => _studentId;
            set => SetValidateProperty(ref _studentId, value);
        }

        public Grade? Grade
        {
            get => _grade;
            set => SetValidateProperty(ref _grade, value);
        }

        public Course Course { get; set; }
        public Student Student { get; set; }
    }

    public class Student : EditableEntityBase
    {
        static Student()
        {
            RegisterModel<Student>(e =>
            {
                e.SetPrimaryKey(nameof(Id));
            });
        }

        private byte[] _rowVersion;
        private int _id;
        private string _lastName;
        private string _firstMidName;
        private DateTime _enrollmentDate;

        [Timestamp]
        [Browsable(false)]
        // ReSharper disable once UnusedMember.Global
        public byte[] RowVersion
        {
            get => _rowVersion;
            set => SetProperty(ref _rowVersion, value);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetValidateProperty(ref _lastName, value);
        }

        public string FirstMidName
        {
            get => _firstMidName;
            set => SetValidateProperty(ref _firstMidName, value);
        }

        [DataType(DataType.Date)]
        public DateTime EnrollmentDate
        {
            get => _enrollmentDate;
            set => SetValidateProperty(ref _enrollmentDate, value);
        }

        public ICollection<Enrollment> Enrollments { get; set; }
    }

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
}
