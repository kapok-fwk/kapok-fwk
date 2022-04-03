using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.Report.DataModel;

/// <summary>
/// A report model is the data structure for a report.
/// </summary>
[Table("ReportModel", Schema = "System")]
public class ReportModel : EditableEntityBase, IEquatable<ReportModel>
{
    static ReportModel()
    {
        RegisterModel<ReportModel>(entity =>
        {
            entity.AddUniqueIndex(nameof(TypeFullName));

            entity.AddOneToManyRelationship<ReportLayout>(nameof(DefaultLayout))
                .HasForeignKey(nameof(DefaultLayoutId))
                .OnDelete(Entity.Model.DeleteBehavior.SetNull);
        });
    }

    private byte[]? _rowVersion;
    private Guid _reportModelId;
    private string _typeFullName;
    private Guid? _defaultLayoutId;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Browsable(false)]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ReportModelId
    {
        get => _reportModelId;
        set => SetValidateProperty(ref _reportModelId, value);
    }

    [StringLength(512)]
    public string TypeFullName
    {
        get => _typeFullName;
        set => SetValidateProperty(ref _typeFullName, value);
    }

    [Browsable(false)]
    public Guid? DefaultLayoutId
    {
        get => _defaultLayoutId;
        set => SetValidateProperty(ref _defaultLayoutId, value);
    }

    public virtual ReportLayout? DefaultLayout { get; set; }
    public virtual ICollection<ReportLayout>? ReportLayouts { get; set; }

    #region IEquatable<ReportModel>

    public bool Equals(ReportModel? other)
    {
        if (other == null) return false;

        return ReportModelId == other.ReportModelId;
    }

    #endregion
}