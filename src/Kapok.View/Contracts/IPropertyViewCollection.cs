using System.Collections;
using System.Collections.Specialized;

namespace Kapok.View;

// ReSharper disable once PossibleInterfaceMemberAmbiguity
public interface IPropertyViewCollection : IList<PropertyView>, IList, INotifyCollectionChanged
{
    IReadOnlyDictionary<string, IPropertyLookupView> LookupViews { get; }

    void RefreshPropertyLookups(bool refreshOnlyDependentOnEntry);
}

public interface IPropertyViewCollection<TEntity> : IPropertyViewCollection
    where TEntity : class
{
    IReadOnlyDictionary<string, IDataSetSelectionAction<TEntity>> DrillDown { get; }
}