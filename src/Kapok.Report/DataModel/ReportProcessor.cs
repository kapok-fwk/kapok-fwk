using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Report.DataModel;

/// <summary>
/// A Report processor is the class running the execution of a report
/// </summary>
[Table("ReportProcessor", Schema = "System")]
public class ReportProcessor : EditableEntityBase, IEquatable<ReportProcessor>
{
    static ReportProcessor()
    {
        RegisterModel<ReportProcessor>(entity =>
        {
            entity.AddUniqueIndex(nameof(TypeFullName));
        });
    }

    private byte[]? _rowVersion;
    private Guid _reportProcessorId;
    private string _typeFullName = string.Empty;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Browsable(false)]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ReportProcessorId
    {
        get => _reportProcessorId;
        set => SetValidateProperty(ref _reportProcessorId, value);
    }

    [StringLength(512)]
    public string TypeFullName
    {
        get => _typeFullName;
        set => SetValidateProperty(ref _typeFullName, value);
    }

    #region IEquatable<ReportModel>

    public bool Equals(ReportProcessor? other)
    {
        if (other == null) return false;

        return ReportProcessorId == other.ReportProcessorId;
    }

    #endregion
}