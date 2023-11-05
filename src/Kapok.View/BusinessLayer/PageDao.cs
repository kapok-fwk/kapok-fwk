using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.View.BusinessLayer;

public interface IPageDao : IDao<DataModel.Page>
{
    Task<DataModel.Page?> FindFromType(string? pageTypeFullName);
    async Task<DataModel.Page?> FindFromType(Type pageType) => await FindFromType(pageType.FullName);
    Task<DataModel.Page> GetOrCreateFromType(Type pageType);
}

public class PageDao : Dao<DataModel.Page>, IPageDao
{
    public PageDao(IDataDomainScope dataDomainScope, IRepository<DataModel.Page> repository) : base(dataDomainScope, repository)
    {
    }

    public async Task<DataModel.Page?> FindFromType(string? pageTypeFullName)
    {
        if (pageTypeFullName == null)
            return null;

        var reportModel = (
            from e in AsQueryable()  // TODO async. implementation missing here
            where e.TypeFullName == pageTypeFullName
            select e
        ).SingleOrDefault();  // TODO async. implementation missing here

        return reportModel;
    }

    public async Task<DataModel.Page> GetOrCreateFromType(Type pageType)
    {
        var reportModel = await FindFromType(pageType.FullName);

        if (reportModel == null)
        {
            reportModel = New();
            reportModel.TypeFullName = pageType.FullName ?? throw new NotSupportedException("pageType.FullName is null");
            await CreateAsync(reportModel);
        }

        return reportModel;
    }
}