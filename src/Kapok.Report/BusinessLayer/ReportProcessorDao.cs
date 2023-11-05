using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Report.DataModel;

namespace Kapok.Report.BusinessLayer;

public interface IReportProcessorDao : IDao<ReportProcessor>
{
    Task<ReportProcessor> GetOrCreateFromType(Type reportProcessorType);
    Type GetType(ReportProcessor entity);
}

public class ReportProcessorDao : Dao<ReportProcessor>, IReportProcessorDao
{
    public ReportProcessorDao(IDataDomainScope dataDomainScope, IRepository<ReportProcessor> repository) : base(dataDomainScope, repository)
    {
    }

    public async Task<ReportProcessor> GetOrCreateFromType(Type reportProcessorType)
    {
        var typeFullName = reportProcessorType.FullName;
        if (string.IsNullOrEmpty(typeFullName))
            throw new ArgumentException($"Could not get FullName of type {reportProcessorType}");

        var reportProcessor = (
            from e in AsQueryable()
            where e.TypeFullName == typeFullName
            select e
        ).SingleOrDefault();

        if (reportProcessor == null)
        {
            reportProcessor = New();
            reportProcessor.ReportProcessorId = Guid.NewGuid();
            reportProcessor.TypeFullName = typeFullName;
            await CreateAsync(reportProcessor);
        }

        return reportProcessor;
    }

    public Type GetType(ReportProcessor entity)
    {
        return TypeHelper.GetTypeFromTypeFullName(entity.TypeFullName);
    }
}