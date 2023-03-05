// ReSharper disable UnusedMember.Global
namespace Kapok.BusinessLayer;

public abstract class BusinessLayerException : Exception
{
    protected BusinessLayerException()
    {
    }

    protected BusinessLayerException(string? message)
        : base(message)
    {
    }

    protected BusinessLayerException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}