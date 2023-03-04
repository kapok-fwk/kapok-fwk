using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Html;

namespace Kapok.Report.Razor;

public abstract class HtmlTemplate : RazorTemplateBase
{
    private readonly StringBuilder _stringBuilder = new();
        
    public override void WriteLiteral(string literal)
    {
        _stringBuilder.Append(literal);
    }

    public override void Write(object? obj)
    {
        if (obj is IHtmlContent htmlContent)
        {
            using var stringWriter = new StringWriter(_stringBuilder);
            htmlContent.WriteTo(stringWriter, HtmlEncoder.Default);
        }
        else if (obj != null)
        {
            _stringBuilder.Append(HttpUtility.HtmlDecode(obj.ToString()));
        }
    }

    public override string Result()
    {
        return _stringBuilder.ToString();
    }

    private readonly Stack<string> _attributeSuffixStack = new();

    public override void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset,
        int attributeValuesCount)
    {
        _stringBuilder.Append(prefix);

        _attributeSuffixStack.Push(suffix);
    }

    public override void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset,
        int valueLength, bool isLiteral)
    {
        _stringBuilder.Append(value);
    }

    public override void EndWriteAttribute()
    {
        _stringBuilder.Append(_attributeSuffixStack.Pop());
    }
}