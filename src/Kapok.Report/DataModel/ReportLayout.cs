using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Kapok.Entity;

namespace Kapok.Report.DataModel;
// TODO: implement an 'IsTemplate' property (or something similar) indicating that this is not changeable by the user and it can be copied from such an template

/// <summary>
/// A report layout is the visible 'Design' for the user.
///
/// E.g. you could have a ReportModel 'SalesDocument' which contains header and lines of a sales doc.
/// Then you probably want to have a Order confirmation, a Pro-Forma Invoice etc. This separation is done via
/// the ReportLayout.
/// </summary>
[Table("ReportLayout", Schema = "System")]
public class ReportLayout : EditableEntityBase, IEquatable<ReportLayout>
{
    static ReportLayout()
    {
        RegisterModel<ReportLayout>(entity =>
        {
            entity.SetPrimaryKey(nameof(ReportLayoutId));

            entity.AddOneToManyRelationship<ReportModel>(nameof(ReportModel))
                .HasForeignKey(nameof(ReportModelId))
                .WithForeignNavigationProperty(nameof(DataModel.ReportModel.ReportLayouts));
            entity.AddOneToManyRelationship<ReportDestination>(nameof(DefaultDestination))
                .HasForeignKey(nameof(ReportLayoutId), nameof(DefaultDestinationId));
            entity.AddOneToManyRelationship<ReportDesign>(nameof(ActiveDesign))
                .HasForeignKey(nameof(ReportLayoutId), nameof(ActiveDesignVersion));
        });
    }

    private byte[]? _rowVersion;
    private Guid _reportLayoutId;
    private Guid _reportModelId;
    private Caption? _name;
    private int? _defaultDestinationId;
    private int? _activeDesignVersion;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Browsable(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ReportLayoutId
    {
        get => _reportLayoutId;
        set => SetValidateProperty(ref _reportLayoutId, value);
    }

    [Browsable(false)]
    public Guid ReportModelId
    {
        get => _reportModelId;
        set => SetValidateProperty(ref _reportModelId, value);
    }

    public Caption? Name
    {
        get => _name;
        set => SetValidateProperty(ref _name, value);
    }

    [Browsable(false)]
    public int? DefaultDestinationId
    {
        get => _defaultDestinationId;
        set => SetValidateProperty(ref _defaultDestinationId, value);
    }

    [Browsable(false)]
    public int? ActiveDesignVersion
    {
        get => _activeDesignVersion;
        set => SetValidateProperty(ref _activeDesignVersion, value);
    }

    public virtual ReportModel? ReportModel { get; set; }
    public virtual ReportDestination? DefaultDestination { get; set; }
    public virtual ReportDesign? ActiveDesign { get; set; }
    public virtual ICollection<ReportDestination>? ReportDestinations { get; set; }
    public virtual ICollection<ReportDesign>? ReportDesigns { get; set; }

    #region IEquatable<ReportLayout>

    public bool Equals(ReportLayout? other)
    {
        if (other == null) return false;

        return ReportLayoutId == other.ReportLayoutId;
    }

    #endregion

    public override string ToString()
    {
        return $"{Name.LanguageOrDefault(CultureInfo.CurrentUICulture)} ({ReportLayoutId:N})";
    }
}