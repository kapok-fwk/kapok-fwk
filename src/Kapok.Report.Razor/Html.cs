using Microsoft.AspNetCore.Html;

namespace Kapok.Report.Razor;

public static class Html
{
    public static IHtmlContent Raw(string @string)
    {
        var html = new HtmlContentBuilder();

        return html.AppendHtml(@string);
    }
}