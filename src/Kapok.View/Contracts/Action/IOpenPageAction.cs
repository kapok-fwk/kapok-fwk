namespace Kapok.View;

public interface IOpenPageAction
{
    /// <summary>
    /// Get or construct the page which shall be opened.
    /// </summary>
    /// <returns></returns>
    IPage GetOrConstructPage();

    /// <summary>
    /// If defined, the page will be opened in this window. Otherwise,
    /// it will be opened in a new window.
    /// </summary>
    DocumentPageCollectionPage HostPage { get; set; }
}