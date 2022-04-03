namespace Kapok.Report.Model;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ReportProcessorAttribute : Attribute
{
    public ReportProcessorAttribute(Type processorType)
    {
        ProcessorType = processorType;
    }

    public Type ProcessorType { get; set; }
}