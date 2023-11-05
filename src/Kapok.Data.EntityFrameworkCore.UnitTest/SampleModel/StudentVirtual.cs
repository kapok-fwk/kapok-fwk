using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;

public class StudentVirtual : EditableEntityBase
{
    static StudentVirtual()
    {
        RegisterModel<StudentVirtual>(e =>
        {
            e.SetPrimaryKey(nameof(Id));
        });
    }

    private byte[]? _rowVersion;
    private int _id;
    private string? _lastName;
    private string? _firstMidName;
    private DateTime _enrollmentDate;

    [Timestamp]
    [Browsable(false)]
    // ReSharper disable once UnusedMember.Global
    public byte[]? RowVersion
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

    public string? LastName
    {
        get => _lastName;
        set => SetValidateProperty(ref _lastName, value);
    }

    public string? FirstMidName
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

    public ICollection<Enrollment>? Enrollments { get; set; }
}