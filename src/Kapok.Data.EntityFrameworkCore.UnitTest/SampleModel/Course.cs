using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;

public class Course : EditableEntityBase
{
    static Course()
    {
        RegisterModel<Course>(e =>
        {
            e.SetPrimaryKey(nameof(CourseId));
        });
    }

    private byte[]? _rowVersion;
    private int _courseId;
    private string _title = string.Empty;
    private int _credits;

    [Timestamp]
    [Browsable(false)]
    // ReSharper disable once UnusedMember.Global
    public byte[]? RowVersion
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

    [Required(AllowEmptyStrings = false)]
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

    public ICollection<Enrollment>? Enrollments { get; set; }
}