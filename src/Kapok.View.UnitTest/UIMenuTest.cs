using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;

namespace Kapok.View.UnitTest;

public class UIMenuTest : ViewDomainUnitTestBase
{
    [Fact]
    public void SerializeEmptyMenuTest()
    {
        var page = new MockupInteractivePage(ServiceProvider);

        var menu = new UIMenu(UIMenu.BaseMenuName, page);

        var jsonString = JsonSerializer.Serialize(menu, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });
        Assert.NotNull(jsonString);
    }

    /*
      This test does not include the OpenPageAction. This needs fixing.

    [Fact]
    public void SerializeSimpleFlatMenuTest()
    {
        var viewDomain = ServiceProvider.GetRequiredService<IViewDomain>();
        var page = new MockupInteractivePage(viewDomain);

        var menu = new UIMenu(UIMenu.BaseMenuName, page);

        var openPageAction = new UIOpenPageAction("OpenPage", typeof(Page), viewDomain);
        menu.MenuItems.Add(new UIMenuItemAction(openPageAction, "OpenPage")
        {
            Description = "This menu item opens a page",
            Image = "TODO",
        });

        var jsonString = JsonSerializer.Serialize(menu, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });
        Assert.NotNull(jsonString);
    }*/
}