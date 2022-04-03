namespace Kapok.View;

public interface IOpenPageAction : IAction
{
    IPage GetOrConstructPage();
}