using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Newtonsoft.Json.Linq;

namespace Kapok.Report.DataModel;

[Table("ReportDestination", Schema = "System")]
public class ReportDestination : EditableEntityBase, IEquatable<ReportDestination>
{
    static ReportDestination()
    {
        RegisterModel<ReportDestination>(entity =>
        {
            entity.AddOneToManyRelationship<ReportLayout>(nameof(ReportLayout))
                .HasForeignKey(nameof(ReportLayoutId))
                .WithForeignNavigationProperty(nameof(DataModel.ReportLayout.ReportDestinations));
        });
    }

    private byte[]? _rowVersion;
    private ReportDestinationType _type;
    private Guid _reportLayoutId;
    private string? _mimeType;
    private int _reportDestinationId;

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
    [Browsable(false)]
    public int ReportDestinationId
    {
        get => _reportDestinationId;
        set => SetValidateProperty(ref _reportDestinationId, value);
    }

    public ReportDestinationType Type
    {
        get => _type;
        set => SetValidateProperty(ref _type, value);
    }

    /// <summary>
    /// Destination mime type, e.g. application/pdf
    /// </summary>
    [StringLength(255)]
    public string? MimeType
    {
        get => _mimeType;
        set => SetValidateProperty(ref _mimeType, value);
    }

    public JObject? ExtendedData { get; set; }

    public virtual ReportLayout? ReportLayout { get; set; }

    #region IEquatable<ReportDestination>

    public bool Equals(ReportDestination? other)
    {
        if (other == null) return false;

        return ReportLayoutId == other.ReportLayoutId &&
               ReportDestinationId == other.ReportDestinationId;
    }

    #endregion
}