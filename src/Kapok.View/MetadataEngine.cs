using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kapok.Data;
using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.Filter;
using Kapok.View.DataModel;

namespace Kapok.View;

public sealed class MetadataEngine
{
    public static MetadataEngine? ActiveMetadataEngine { get; set; }

    private readonly IDataDomain? _dataDomain;

    public MetadataEngine(IDataDomain? dataDomain = default, IIdentity? currentUserIdentity = null)
    {
        _dataDomain = dataDomain;

        if (currentUserIdentity != null && currentUserIdentity.GetType().Name == "DbUserIdentity")
        {
            // NOTE: this assumes that you use the Kapok.Acl module. We use reflection because we do not want to
            //       add it as a hard reference here.
            var dbUser = currentUserIdentity.GetType().GetProperty("DbUser")?.GetMethod
                ?.Invoke(currentUserIdentity, Array.Empty<object>());
            if (dbUser != null)
            {
                var userIdObj = dbUser.GetType().GetProperty("Id")?.GetMethod?.Invoke(dbUser, Array.Empty<object>());
                if (userIdObj is Guid userId)
                {
                    CurrentUserId = userId;
                }
            }
        }

        // If no global metadata manager is set, do it now.
        ActiveMetadataEngine ??= this;
    }

    public Guid? CurrentUserId { get; }

    private void CheckDataDomainSet([CallerMemberName] string? memberName = null)
    {
        if (_dataDomain == null)
            throw new NotSupportedException($"You have to instantiate the MetadataEngine with the dataDomain parameter to be able to call method {memberName}");
    }

    private void CheckCurrentUserIdSet([CallerMemberName] string? memberName = null)
    {
        if (CurrentUserId == null)
            throw new NotSupportedException(
                $"You have to instantiate the MetadataEngine with the currentUserIdentity parameter using the Kapok.Acl IIdentity instance to be able to call method {memberName}");
    }

    private async Task<DataModel.Page?> FindDataModelPage(Type pageType, IDataDomainScope dataDomainScope)
    {
        return await dataDomainScope.GetEntityService<DataModel.Page, BusinessLayer.IPageEntityService>()
            .FindFromType(pageType);
    }

    private async Task<DataModel.Page> GetOrCreateDataModelPage(Type pageType, IDataDomainScope dataDomainScope)
    {
        return await dataDomainScope.GetEntityService<DataModel.Page, BusinessLayer.IPageEntityService>()
            .GetOrCreateFromType(pageType);
    }

    public async ValueTask<string?> GetPageMetadata(Type pageType)
    {
        if (_dataDomain == null)
            return null;

        if (CurrentUserId == null)
            return null;

        using var dataDomainScope = _dataDomain.CreateScope();
        var pageDataModel = await FindDataModelPage(pageType, dataDomainScope);
        if (pageDataModel == null)
            return null;
        var pageUserMetadata = await dataDomainScope.GetEntityService<UserPageMetadata>().FindByKeyAsync(CurrentUserId, pageDataModel.PageId);
        await dataDomainScope.SaveAsync();
        return pageUserMetadata?.Data;
    }

    public async Task SavePageMetadata(Type pageType, string metadata)
    {
        CheckDataDomainSet();
        CheckCurrentUserIdSet();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        var pageDataModel = await GetOrCreateDataModelPage(pageType, dataDomainScope);
        var userPageMetadataService = dataDomainScope.GetEntityService<UserPageMetadata>();
        var pageUserMetadata = userPageMetadataService.FindByKeyForUpdate(CurrentUserId, pageDataModel.PageId);
        if (pageUserMetadata != null)
        {
            pageUserMetadata.Data = metadata;
        }
        else
        {
            pageUserMetadata = userPageMetadataService.New();
#pragma warning disable CS8629 // Nullable value type may be null.
            pageUserMetadata.UserId = CurrentUserId.Value;
#pragma warning restore CS8629 // Nullable value type may be null.
            pageUserMetadata.PageId = pageDataModel.PageId;
            await userPageMetadataService.CreateAsync(pageUserMetadata);
        }
        await dataDomainScope.SaveAsync();
    }

    public async IAsyncEnumerable<DataSetListView> GetPageListViews(Type pageType, Type entityType)
    {
        if (_dataDomain == null)
            yield break;

        var serializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters =
            {
                new IFilterJsonConverter()
            }
        };

        using var dataDomainScope = _dataDomain.CreateScope();
        var pageDataModel = await GetOrCreateDataModelPage(pageType, dataDomainScope);
        var listViewsSerivce = dataDomainScope.GetEntityService<PageListView>();
        foreach (var rawListViewData in from lv in listViewsSerivce.AsQueryable()
                                         where lv.PageId == pageDataModel.PageId
                                         select new
                                         {
                                             lv.Data
                                         })
        {
            yield return (DataSetListView)JsonSerializer.Deserialize(rawListViewData.Data, typeof(DataSetListView),
                serializerOptions)!;
        }

        // user list views
        if (CurrentUserId.HasValue)
        {
            var userListViewsService = dataDomainScope.GetEntityService<UserPageListView>();
            foreach (var rawListViewData in from lv in userListViewsService.AsQueryable()
                                             where lv.PageId == pageDataModel.PageId &&
                                                   lv.UserId == CurrentUserId.Value
                                             select new
                                             {
                                                 lv.Data
                                             })
            {
                yield return (DataSetListView)JsonSerializer.Deserialize(rawListViewData.Data, typeof(DataSetListView),
                    serializerOptions)!;
            }
        }
    }

    /// <summary>
    /// Saves a dataSetListView in the database. If it already exists, it will be updated with the new version.
    /// </summary>
    /// <param name="pageType"></param>
    /// <param name="dataSetListView"></param>
    /// <param name="userSpecific">
    /// If the DataSetListView shall be written as user specific. Otherwise it is
    /// accessible to everyone.
    /// </param>
    public async Task SavePageListView(Type pageType, DataSetListView dataSetListView, bool userSpecific)
    {
        CheckDataDomainSet();
        if (userSpecific)
            CheckCurrentUserIdSet();
        if (string.IsNullOrEmpty(dataSetListView.Name))
            throw new ArgumentException("The Name property must be set of dataSetListView", nameof(dataSetListView));

        var jsonData = JsonSerializer.Serialize(dataSetListView);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        var pageDataModel = await GetOrCreateDataModelPage(pageType, dataDomainScope);

        if (userSpecific)
        {
            var service = dataDomainScope.GetEntityService<UserPageListView>();
            var lv = await service.FindByKeyAsync(CurrentUserId.Value, pageDataModel.PageId, dataSetListView.Name);
            if (lv == null)
            {
                lv.Data = jsonData;
                await dataDomainScope.SaveAsync();
                return;
            }

            lv = service.New();
            lv.UserId = CurrentUserId.Value;
            lv.PageId = pageDataModel.PageId;
            lv.Name = dataSetListView.Name;
            lv.Data = jsonData;
            await service.CreateAsync(lv);
            await dataDomainScope.SaveAsync();
        }
        else
        {
            var service = dataDomainScope.GetEntityService<PageListView>();
            var lv = await service.FindByKeyAsync(pageDataModel.PageId, dataSetListView.Name);
            if (lv == null)
            {
                lv.Data = jsonData;
                await dataDomainScope.SaveAsync();
                return;
            }
            lv = service.New();
            lv.PageId = pageDataModel.PageId;
            lv.Name = dataSetListView.Name;
            lv.Data = jsonData;
            await service.CreateAsync(lv);
            await dataDomainScope.SaveAsync();
        }
    }

    // TODO: implementation for ListPage, UserPageListView, UserPageMenu and UserPageMetadata
}