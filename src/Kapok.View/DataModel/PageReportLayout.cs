using System.ComponentModel;
using Kapok.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.View.DataModel;

[Table(nameof(PageReportLayout), Schema = "System")]
public class PageReportLayout : EditableEntityBase
{
    static PageReportLayout()
    {
        RegisterModel<PageReportLayout>(entity =>
        {
            entity.AddOneToManyRelationship<Page>()
                .HasForeignKey(nameof(PageId));
        });
    }

    private Guid _pageId;
    private Guid _reportLayoutId;

    [Key]
    [Browsable(false)]
    public Guid PageId
    {
        get => _pageId;
        set => SetProperty(ref _pageId, value);
    }

    [Key]
    [Browsable(false)]
    public Guid ReportLayoutId
    {
        get => _reportLayoutId;
        set => SetValidateProperty(ref _reportLayoutId, value);
    }
}