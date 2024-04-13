using Kapok.Data;
using Kapok.Module;

namespace Kapok.View;

/// <summary>
/// Module definition for the ViewMetadataModule, a data module holding metadata information for UI pages.
/// </summary>
public sealed class ViewMetadataModule : ModuleBase
{
    public ViewMetadataModule() : base(nameof(ViewMetadataModule))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        // register DataModel
        DataDomain.RegisterEntity<DataModel.Page, BusinessLayer.PageEntityService>();
        DataDomain.RegisterEntity<DataModel.PageListView>();
        DataDomain.RegisterEntity<DataModel.PageReportLayout>();
        DataDomain.RegisterEntity<DataModel.UserPageListView>();
        DataDomain.RegisterEntity<DataModel.UserPageMenu>();
        DataDomain.RegisterEntity<DataModel.UserPageMetadata>();
    }
}