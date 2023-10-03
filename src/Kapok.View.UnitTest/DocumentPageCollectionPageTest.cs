using Xunit;

namespace Kapok.View.UnitTest;

public class DocumentPageCollectionPageTest
{
    private UnitTestViewDomain ViewDomain { get; }
    
    public DocumentPageCollectionPageTest()
    {
        ViewDomain = new UnitTestViewDomain();
    }

    [Fact]
    public void DocumentMenuLinkTest()
    {
        var hostPage = new MockupDocumentPageCollectionPage(ViewDomain);

        Assert.Null(hostPage.CurrentDocumentPage);
        Assert.Single(hostPage.Menu[UIMenu.BaseMenuName].MenuItems);
        Assert.Empty(hostPage.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems);
        
        var page = new MockupPage(ViewDomain)
        {
            Title = "EmptyPage"
        };
        hostPage.DocumentPages.Add(page);
        
        var interactivePage1 = new MockupInteractivePage(ViewDomain)
        {
            Title = "SimpleMenu1",
        };
        interactivePage1.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems.Add(new UIMenuItem("MenuItem1.1"));
        interactivePage1.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems.Add(new UIMenuItem("MenuItem1.2"));
        hostPage.DocumentPages.Add(interactivePage1);
        
        var interactivePage2 = new MockupInteractivePage(ViewDomain)
        {
            Title = "SimpleMenu2",
        };
        interactivePage2.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems.Add(new UIMenuItem("MenuItem2.1"));
        interactivePage2.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems.Add(new UIMenuItem("MenuItem2.2"));
        hostPage.DocumentPages.Add(interactivePage2);
        
        Assert.Null(hostPage.CurrentDocumentPage);
        Assert.Single(hostPage.Menu[UIMenu.BaseMenuName].MenuItems);
        Assert.Empty(hostPage.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems);
        
        // test out switching the active document

        hostPage.CurrentDocumentPage = page;
        Assert.Single(hostPage.Menu[UIMenu.BaseMenuName].MenuItems);
        Assert.Empty(hostPage.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems);
        
        hostPage.CurrentDocumentPage = interactivePage1;
        Assert.Equal(2, hostPage.Menu[UIMenu.BaseMenuName].MenuItems.Count);
        Assert.Equal(2, hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems.Count);
        Assert.Equal("MenuItem1.1", hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems[0].Name);
        Assert.Equal("MenuItem1.2", hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems[1].Name);
        
        hostPage.CurrentDocumentPage = interactivePage2;
        Assert.Equal(2, hostPage.Menu[UIMenu.BaseMenuName].MenuItems.Count);
        Assert.Equal(2, hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems.Count);
        Assert.Equal("MenuItem2.1", hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems[0].Name);
        Assert.Equal("MenuItem2.2", hostPage.Menu[UIMenu.BaseMenuName].MenuItems[1].SubMenuItems[1].Name);

        hostPage.CurrentDocumentPage = null;
        Assert.Single(hostPage.Menu[UIMenu.BaseMenuName].MenuItems);
        Assert.Empty(hostPage.Menu[UIMenu.BaseMenuName].MenuItems[0].SubMenuItems);
    }

    [Fact]
    void MenuPatching()
    {
        var hostPage = new MockupDocumentPageCollectionPage(ViewDomain);

        var openPageMenuItem =
            new UIMenuItemAction(new UIOpenPageAction("OpenPageMenuItem1", typeof(Page), ViewDomain));
        Assert.NotNull(openPageMenuItem.Action);

        Assert.Null(((UIOpenPageAction)openPageMenuItem.Action).HostPage);

        hostPage.Menu[UIMenu.BaseMenuName].MenuItems.Add(openPageMenuItem);

        hostPage.PatchMenuToOpenHere(UIMenu.BaseMenuName);
        Assert.NotNull(((UIOpenPageAction)openPageMenuItem.Action).HostPage);
        Assert.Equal(hostPage, ((UIOpenPageAction)openPageMenuItem.Action).HostPage);
    }
}