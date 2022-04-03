using System.Data;

namespace Kapok.Report;

public interface IDataTableReportProcessor : IReportProcessor
{
    DataTable ProcessToDataTable();
}