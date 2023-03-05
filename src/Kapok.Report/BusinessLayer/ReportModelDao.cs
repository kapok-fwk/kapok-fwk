using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportModelDao : IDao<ReportModel>
{
    ReportModel GetOrCreateFromType(Type reportModelType);
    Type GetType(ReportModel entity);
}

public class ReportModelDao : Dao<ReportModel>, IReportModelDao
{
    public ReportModelDao(IDataDomainScope dataDomainScope) : base(dataDomainScope)
    {
    }

    public ReportModel GetOrCreateFromType(Type reportModelType)
    {
        var typeFullName = reportModelType.FullName;
        if (string.IsNullOrEmpty(typeFullName))
            throw new ArgumentException($"Could not get FullName of type {reportModelType}");

        var reportModel = (
            from e in AsQueryable()
            where e.TypeFullName == typeFullName
            select e
        ).SingleOrDefault();

        if (reportModel == null)
        {
            reportModel = New();
            reportModel.ReportModelId = Guid.NewGuid();
            reportModel.TypeFullName = typeFullName;
            Create(reportModel);
        }

        return reportModel;
    }

    public Type GetType(ReportModel entity)
    {
        return TypeHelper.GetTypeFromTypeFullName(entity.TypeFullName);
    }
}