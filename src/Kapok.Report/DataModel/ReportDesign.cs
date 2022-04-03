using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Report.DataModel;

/// <summary>
/// A report design is a version of a report layout.
/// </summary>
[Table("ReportDesign", Schema = "System")]
public class ReportDesign : EditableEntityBase, IEquatable<ReportDesign>
{
    static ReportDesign()
    {
        RegisterModel<ReportDesign>(entity =>
        {
            entity.AddOneToManyRelationship<ReportLayout>(nameof(ReportLayout))
                .HasForeignKey(nameof(ReportLayoutId))
                .WithForeignNavigationProperty(nameof(DataModel.ReportLayout.ReportDesigns));
            entity.AddOneToManyRelationship<ReportProcessor>(nameof(ReportProcessor))
                .HasForeignKey(nameof(ReportProcessorId));

        });
    }

    private byte[]? _rowVersion;
    private Guid _reportLayoutId;
    private int _versionNum;
    private DateTime _versionDateTime;
    private Guid _reportProcessorId;
    private byte[]? _designData;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Key]
    [Browsable(false)]
    public Guid ReportLayoutId
    {
        get => _reportLayoutId;
        set => SetValidateProperty(ref _reportLayoutId, value);
    }

    [Key]
    public int VersionNum
    {
        get => _versionNum;
        set => SetValidateProperty(ref _versionNum, value);
    }

    [AutoGenerateValue(AutoGenerateValueType.CreatedDateTime)]
    public DateTime VersionDateTime
    {
        get => _versionDateTime;
        set => SetValidateProperty(ref _versionDateTime, value);
    }

    [Browsable(false)]
    public Guid ReportProcessorId
    {
        get => _reportProcessorId;
        set => SetValidateProperty(ref _reportProcessorId, value);
    }

    public byte[]? DesignData
    {
        get => _designData;
        set => SetValidateProperty(ref _designData, value);
    }

    public virtual ReportLayout? ReportLayout { get; set; }

    public virtual ReportProcessor? ReportProcessor { get; set; }

    #region IEquatable<ReportDesign>

    public bool Equals(ReportDesign? other)
    {
        if (other == null) return false;

        return ReportLayoutId == other.ReportLayoutId &&
               VersionNum == other.VersionNum;
    }

    #endregion
}