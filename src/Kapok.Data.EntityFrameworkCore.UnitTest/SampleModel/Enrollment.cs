using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;

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

    private byte[]? _rowVersion;
    private int _enrollmentId;
    private int _courseId;
    private int _studentId;
    private Grade? _grade;

    [Timestamp]
    [Browsable(false)]
    // ReSharper disable once UnusedMember.Global
    public byte[]? RowVersion
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

    public Course? Course { get; set; }
    public Student? Student { get; set; }
}