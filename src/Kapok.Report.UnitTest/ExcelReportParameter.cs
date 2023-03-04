using System.Globalization;
using Xunit;
using Assert = Xunit.Assert;

namespace Kapok.Report.UnitTest;

public class ExcelReportParameter
{
    [Fact]
    public void ExcelReport_ReplaceReportParameters()
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        var result = ExcelReport.ReplaceReportParameters("{variable} YTD", getVariableValue: name =>
            {
                Assert.Equal("variable", name);
                return "ACC";
            },
            cultureInfo: CultureInfo.GetCultureInfo("en-US"));
        Assert.Equal("ACC YTD", result);
    }
}