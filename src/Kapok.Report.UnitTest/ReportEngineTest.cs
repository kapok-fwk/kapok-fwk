using Kapok.Data.EntityFrameworkCore.UnitTest;
using Kapok.Module;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kapok.Report.UnitTest;

public class TestExcelReportSheet : ExcelReport
{
    public TestExcelReportSheet()
    {
        Name = "Sheet_1";
        this.Caption = "Sheet 1";
    }
}

public class TextExcelReport : ExcelReportPackage
{
    public TextExcelReport()
    {
        Name = nameof(TextExcelReport);

        SubReports.Add(new TestExcelReportSheet());
    }
}

public class ReportEngineTest : DeferredEntityServiceTestBase
{
    protected override void InitiateModule()
    {
        ModuleEngine.InitiateModule(typeof(ReportModule));
        ExcelReportPackageProcessor.Register();
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        base.ConfigureServices(serviceCollection);

        serviceCollection.AddSingleton<ReportEngine>();
    }

    [Fact]
    public void TestEmptyExcelReport()
    {
        var reportEngine = ServiceProvider.GetRequiredService<ReportEngine>();
        var report = new TextExcelReport();

        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);

        reportEngine.ExecuteReport(report, new Dictionary<string, object>(), ExcelReportPackageProcessor.XlsxMimeType, streamWriter.BaseStream);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var streamReader = new StreamReader(memoryStream);

        var result = streamReader.ReadToEnd();
        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, result);
    }
}