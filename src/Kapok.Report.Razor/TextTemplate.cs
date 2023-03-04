using System.Runtime.CompilerServices;
using System.Text;

namespace Kapok.Report.Razor;

public abstract class TextTemplate : RazorTemplateBase
{
    private readonly StringBuilder _stringBuilder = new();
        
    public override void WriteLiteral(string literal)
    {
        _stringBuilder.Append(literal);
    }

    public override void Write(object? obj)
    {
        _stringBuilder.Append(obj);
    }

    public override string Result()
    {
        return _stringBuilder.ToString();
    }


    #region Not supported methods

    private void ThrowMethodNotSupportedException([CallerMemberName] string? methodName = default)
    {
        throw new NotSupportedException(
            $"Method {methodName} is not supported for {nameof(TextTemplate)}");
    }

        
    public override void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset,
        int attributeValuesCount)
    {
        ThrowMethodNotSupportedException();
    }

    public override void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset,
        int valueLength, bool isLiteral)
    {
        ThrowMethodNotSupportedException();
    }

    public override void EndWriteAttribute()
    {
        ThrowMethodNotSupportedException();
    }

    #endregion
}