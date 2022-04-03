using System.Linq.Expressions;
using Kapok.Core;

namespace Kapok.Entity;

public interface ILookupDefinition
{
    Func<object, IDataDomainScope, IQueryable<object>> EntriesFunc { get; set; }
    Expression<Func<object, object>> FieldSelectorFunc { get; set; }
    bool EntriesFuncDependentOnEntry { get; }
}

public interface ILookupDefinition<TBaseEntry, TLookupEntry, TFieldType> : ILookupDefinition
    where TBaseEntry : class
    where TLookupEntry : class
{
    new Func<TBaseEntry, IDataDomainScope, IQueryable<TLookupEntry>> EntriesFunc { get; set; }
    new Expression<Func<TLookupEntry, TFieldType>> FieldSelectorFunc { get; set; }
    new bool EntriesFuncDependentOnEntry { get; }
}

public class LookupDefinition<TBaseEntry, TLookupEntry, TFieldType> : ILookupDefinition<TBaseEntry, TLookupEntry, TFieldType>, ILookupDefinition
    where TBaseEntry : class
    where TLookupEntry : class
{
    public LookupDefinition(Func<IDataDomainScope, IQueryable<TLookupEntry>> lookupEntriesFunc)
    {
        EntriesFunc = (entry, repository) => lookupEntriesFunc.Invoke(repository);
        EntriesFuncDependentOnEntry = false;
    }

    public LookupDefinition(Func<TBaseEntry, IDataDomainScope, IQueryable<TLookupEntry>> lookupEntriesFunc)
    {
        EntriesFunc = lookupEntriesFunc;
        EntriesFuncDependentOnEntry = true;
    }

    public LookupDefinition(Func<IDataDomainScope, IQueryable<TLookupEntry>> lookupEntriesFunc, Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
    {
        EntriesFunc = (entry, repository) => lookupEntriesFunc.Invoke(repository);
        EntriesFuncDependentOnEntry = false;
        FieldSelectorFunc = fieldSelector;
    }

    public LookupDefinition(Func<TBaseEntry, IDataDomainScope, IQueryable<TLookupEntry>> lookupEntriesFunc, Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
    {
        EntriesFunc = lookupEntriesFunc;
        EntriesFuncDependentOnEntry = true;
        FieldSelectorFunc = fieldSelector;
    }

    public bool EntriesFuncDependentOnEntry { get; }

    public Func<TBaseEntry, IDataDomainScope, IQueryable<TLookupEntry>>? EntriesFunc { get; set; }

    public Expression<Func<TLookupEntry, TFieldType>>? FieldSelectorFunc { get; set; }

    #region ILookupDefinition
        
    Func<object, IDataDomainScope, IQueryable<object>>? ILookupDefinition.EntriesFunc
    {
        get
        {
            if (EntriesFunc == null)
                return null;

            return (entry, dataDomainScope) => EntriesFunc.Invoke((TBaseEntry)entry, dataDomainScope);
        }
        set
        {
            if (value == null)
            {
                EntriesFunc = null;
                return;
            }
                
            EntriesFunc = (entry, dataDomainScope) => value.Invoke(entry, dataDomainScope)?.Cast<TLookupEntry>();
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