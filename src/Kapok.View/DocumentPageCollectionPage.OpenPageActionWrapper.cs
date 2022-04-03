using System.Diagnostics;

namespace Kapok.View;

public partial class DocumentPageCollectionPage
{
    /// <summary>
    /// This wrapper class is used to override the basis open-page action with a new one,
    /// which creates the new page and then adds the page to the document pages.
    /// </summary>
    internal class OpenPageActionWrapper : IOpenPageAction
    {
        private readonly IOpenPageAction _baseAction;
        private readonly DocumentPageCollectionPage _page;

        public OpenPageActionWrapper(IOpenPageAction baseAction, DocumentPageCollectionPage page)
        {
            _baseAction = baseAction;
            _page = page;
        }

        #region IOpenPageAction

        string IAction.Name => _baseAction.Name;

        string IAction.Image => _baseAction.Image;

        bool? IAction.ImageIsBig => _baseAction.ImageIsBig;

        bool IAction.IsVisible
        {
            get => _baseAction.IsVisible;
            set => _baseAction.IsVisible = value;
        }

        event EventHandler IAction.CanExecuteChanged
        {
            add => _baseAction.CanExecuteChanged += value;
            remove => _baseAction.CanExecuteChanged -= value;
        }

        bool IAction.CanExecute()
        {
            return _baseAction.CanExecute();
        }

        void IAction.Execute()
        {
            var existingDocumentPage = _page.FindDocumentPageBySource(_baseAction);
            if (existingDocumentPage != null)
            {
                _page.SelectedDocumentPage = existingDocumentPage;
                return;
            }

            var newPage = _baseAction.GetOrConstructPage();
            Debug.Assert(newPage != null);

            _page.AddDocumentPageWithSource(newPage, _baseAction);
            _page.SelectedDocumentPage = newPage;
        }

        IPage IOpenPageAction.GetOrConstructPage()
        {
            return _baseAction.GetOrConstructPage();
        }

        #endregion
    }
}