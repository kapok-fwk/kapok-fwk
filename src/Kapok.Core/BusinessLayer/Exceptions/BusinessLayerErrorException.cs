namespace Kapok.Core;

/// <summary>
/// A exception which is called when 'ReportError(..)' in business layer is called and 'ThrowOnError' is set to true.
///
/// When this exception is called, the error has already been reported, so, no need in a try/catch statement to again report
/// this error.
/// </summary>
public class BusinessLayerErrorException : Exception
{
    public BusinessLayerErrorException(string message) : base(message)
    {
    }
}