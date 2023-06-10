using System.Linq.Expressions;
using Kapok.Data;

namespace Kapok.Entity.Model;

/// <summary>
/// Represents a definition what possible items to show for a property.
/// </summary>
public interface ILookupDefinition
{
    /// <summary>
    /// A function returning the list of possible items in the combobox dropdown.
    ///
    /// The first <b><c>object?</c></b> parameter of the function represents the current item if set. This is only passed when <see cref="EntriesFuncDependentOnEntry"/> is <c>true</c>.
    /// The second <b><c>IDataDomainScope</c></b> parameter holds a data domain scope created for this call which
    /// can be used to access entity lists of the data domain. 
    /// </summary>
    Func<object?, IDataDomainScope, IEnumerable<object>> EntriesFunc { get; set; }

    /// <summary>
    /// The field selector returning the property value to be written to the property.
    ///
    /// The first <b><c>object</c></b> parameter represents the entity retrieved while iterating through <see cref="EntriesFunc"/>.
    /// </summary>
    Expression<Func<object, object>>? FieldSelectorFunc { get; set; }
    
    /// <summary>
    /// If the lookup is dynamic and depends on properties of the current item.
    /// </summary>
    bool EntriesFuncDependentOnEntry { get; }
}

/// <summary>
/// Represents a definition what possible items to show for a property.
/// </summary>
/// <typeparam name="TBaseEntry">entity type for which the property lookup is performed</typeparam>
/// <typeparam name="TLookupEntry">lookup type</typeparam>
/// <typeparam name="TFieldType">type of the returned value. Should match the type of the property the lookup is performed for</typeparam>
public interface ILookupDefinition<TBaseEntry, TLookupEntry, TFieldType> : ILookupDefinition
    where TBaseEntry : class
    where TLookupEntry : class
{
    /// <summary>
    /// A function returning the list of possible items in the combobox dropdown.
    ///
    /// The first <b><c>TBaseEntry</c></b> parameter of the function represents the current item if set. This is only passed when <see cref="ILookupDefinition.EntriesFuncDependentOnEntry"/> is <c>true</c>.
    /// The second <b><c>IDataDomainScope</c></b> parameter holds a data domain scope created for this call which
    /// can be used to access entity lists of the data domain. 
    /// </summary>
    new Func<TBaseEntry?, IDataDomainScope, IEnumerable<TLookupEntry>> EntriesFunc { get; set; }
    
    /// <summary>
    /// The field selector returning the property value to be written to the property.
    ///
    /// The first <b><c>TLookupEntry</c></b> parameter represents the entity retrieved while iterating through <see cref="EntriesFunc"/>.
    /// </summary>
    new Expression<Func<TLookupEntry, TFieldType>>? FieldSelectorFunc { get; set; }
}

public class LookupDefinition<TBaseEntry, TLookupEntry, TFieldType> : ILookupDefinition<TBaseEntry, TLookupEntry, TFieldType>
    where TBaseEntry : class
    where TLookupEntry : class
{
    public LookupDefinition(Func<IDataDomainScope, IEnumerable<TLookupEntry>> lookupEntriesFunc)
    {
        EntriesFunc = (_, repository) => lookupEntriesFunc.Invoke(repository);
        EntriesFuncDependentOnEntry = false;
    }

    public LookupDefinition(Func<TBaseEntry?, IDataDomainScope, IEnumerable<TLookupEntry>> lookupEntriesFunc)
    {
        EntriesFunc = lookupEntriesFunc;
        EntriesFuncDependentOnEntry = true;
    }

    public LookupDefinition(Func<IDataDomainScope, IEnumerable<TLookupEntry>> lookupEntriesFunc, Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
    {
        EntriesFunc = (_, repository) => lookupEntriesFunc.Invoke(repository);
        EntriesFuncDependentOnEntry = false;
        FieldSelectorFunc = fieldSelector;
    }

    public LookupDefinition(Func<TBaseEntry?, IDataDomainScope, IEnumerable<TLookupEntry>> lookupEntriesFunc, Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
    {
        EntriesFunc = lookupEntriesFunc;
        EntriesFuncDependentOnEntry = true;
        FieldSelectorFunc = fieldSelector;
    }

    public bool EntriesFuncDependentOnEntry { get; }

    public Func<TBaseEntry?, IDataDomainScope, IEnumerable<TLookupEntry>> EntriesFunc { get; set; }

    public Expression<Func<TLookupEntry, TFieldType>>? FieldSelectorFunc { get; set; }

    #region ILookupDefinition
        
    Func<object?, IDataDomainScope, IEnumerable<object>> ILookupDefinition.EntriesFunc
    {
        get
        {
            return (entry, dataDomainScope) => EntriesFunc.Invoke((TBaseEntry?)entry, dataDomainScope);
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            EntriesFunc = (entry, dataDomainScope) => value.Invoke(entry, dataDomainScope).Cast<TLookupEntry>();
        }
    }

    Expression<Func<object, object>>? ILookupDefinition.FieldSelectorFunc
    {
        get
        {
            if (FieldSelectorFunc == null)
                return null;

            Expression converted = Expression.Convert(FieldSelectorFunc.Body, typeof(object));
            return Expression.Lambda<Func<object, object>>(converted, Expression.Parameter(typeof(object), "entry"));
        }
        set
        {
            if (value == null)
            {
                FieldSelectorFunc = null;
                return;
            }

            FieldSelectorFunc = entry => (TFieldType)value.Compile().Invoke(entry);
        }
    }

    #endregion
}