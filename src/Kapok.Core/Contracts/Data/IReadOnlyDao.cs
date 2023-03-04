using Kapok.Data;
using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

public interface IReadOnlyDao<T>
    where T : class
{
    /// <summary>
    /// The data domain scope this dao object belongs to.
    /// </summary>
    IDataDomainScope? DataDomainScope { get; }

    /// <summary>
    /// Here you can specify which nested data of the `T` object shall be explicitly loaded.
    /// </summary>
    ICollection<string>? IncludeNestedData { get; }

    IQueryable<T> AsQueryable();


    IEntityModel Model { get; }

    IFilterSet<T> Filter { get; }

    /// <summary>
    /// Returns a IQueryable&lt;T&gt; to a referenced nested entity.
    /// </summary>
    /// <typeparam name="TNested">Nested entity type</typeparam>
    /// <param name="entity">The base entity for which the nested entity query shall be created.</param>
    /// <param name="referenceName">(Optional) The reference name for the nested entity if the nested entity type is referenced multiple times.</param>
    /// <returns></returns>
    IQueryable<TNested> GetNestedAsQueryable<TNested>(T entity, string? referenceName = null)
        where TNested : class, new();
}