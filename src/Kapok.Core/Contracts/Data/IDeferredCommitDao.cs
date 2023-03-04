using Kapok.Entity.Model;

namespace Kapok.BusinessLayer;

/// <summary>
/// A dao which commits/saves the changes only on request.
/// </summary>
public interface IDeferredCommitDao
{
    IEntityModel Model { get; }

    // TODO: implement async methods as well!
    bool CanSave();
    void Save();
    void RejectChanges();

    /// <summary>
    /// This method is called after save is successfully called.
    ///
    /// In this method you can implement post save actions e.g. getting an update of an entity when it
    /// has database generated values
    /// </summary>
    void PostSave();

    /// <summary>
    /// Start update change tracking of an entity
    /// </summary>
    /// <param name="entity">
    /// The entity object.
    /// </param>
    void StartChangeTracking(object entity);
}