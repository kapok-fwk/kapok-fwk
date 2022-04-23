using Kapok.Core;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIOpenReferencedCardPageAction<TEntry> : UIOpenReferencedPageAction<TEntry>
    where TEntry : class, new()
{
    public UIOpenReferencedCardPageAction(string name, ICardPage page,
        Func<TEntry, bool>? canExecute = null)
        : base(name, page, canExecute: canExecute)
    {
    }

    public UIOpenReferencedCardPageAction(string name, Type pageType, IViewDomain viewDomain,
        IDataSetView<TEntry>? baseDataSetView = null,
        Func<TEntry, bool>? canExecute = null,
        IDataDomainScope? dataDomainScope = null)
        : base(name, pageType, viewDomain, baseDataSetView, canExecute: canExecute)
    {
        // main constructor code
        if (!typeof(ICardPage).IsAssignableFrom(pageType))
            throw new ArgumentException($"The {nameof(pageType)} parameter must have a type which implements the interface {typeof(ICardPage<>).FullName}", nameof(pageType));

        if (dataDomainScope != null)
#pragma warning disable CS8602
            PageConstructorParamValues.Add(typeof(IDataDomainScope), dataDomainScope);
#pragma warning restore CS8602
    }
}