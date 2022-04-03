using System.Threading.Tasks;

namespace Kapok.Report.Razor
{
    // TODO: not all members of RazorPage (https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.razor.razorpage?view=aspnetcore-1.1) are implemented!
    public abstract class RazorTemplateBase
    {
        public dynamic? Model { get; set; }

        public abstract Task ExecuteAsync();

        public abstract void WriteLiteral(string literal);
        public abstract void Write(object? obj);
        public abstract string Result();

        public abstract void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount);
        public abstract void WriteAttributeValue(string prefix, int prefixOffset, object value, int valueOffset, int valueLength, bool isLiteral);
        public abstract void EndWriteAttribute();
    }
}