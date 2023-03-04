using System.Globalization;
using System.IO;
using Xunit;

namespace Kapok.Report.Razor.UnitTest;

public class HtmlReportProcessorTest
{
    public class SampleHtmlModel : Model.HtmlReport
    {
        public SampleHtmlModel()
        {
            Name = "SampleHtmlModel";
            Template = "<h1>Hello @Model.PropertyName!</h1>";
        }

        public Caption PropertyName => new Caption
        {
            {string.Empty, "--World--"}, // invariant culture string (= default string)
            {"en-US", "World"},
            {"de-DE", "Welt"},
        };
    }

    [Fact]
    public void TestInvariantCulture()
    {
        var processor = new HtmlReportProcessor();
        processor.ReportModel = new SampleHtmlModel();
        processor.ReportLanguage = CultureInfo.InvariantCulture;

        using var stream = new MemoryStream();

        processor.ProcessToStream(HtmlReportProcessor.HtmlMimeType, stream);

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var htmlContent = reader.ReadToEnd();

        Assert.Equal("<h1>Hello --World--!</h1>", htmlContent);
    }
        
    [Fact]
    public void TestEnglishLanguage()
    {
        var processor = new HtmlReportProcessor();
        processor.ReportModel = new SampleHtmlModel();
        processor.ReportLanguage = CultureInfo.GetCultureInfo("en-US");
            
        using var stream = new MemoryStream();
            
        processor.ProcessToStream(HtmlReportProcessor.HtmlMimeType, stream);

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var htmlContent = reader.ReadToEnd();

        Assert.Equal("<h1>Hello World!</h1>", htmlContent);
    }
        
    [Fact]
    public void TestGermanLanguage()
    {
        var processor = new HtmlReportProcessor();
        processor.ReportModel = new SampleHtmlModel();
        processor.ReportLanguage = CultureInfo.GetCultureInfo("de-DE");
            
        using var stream = new MemoryStream();
            
        processor.ProcessToStream(HtmlReportProcessor.HtmlMimeType, stream);

        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        var htmlContent = reader.ReadToEnd();

        Assert.Equal("<h1>Hello Welt!</h1>", htmlContent);
    }
}