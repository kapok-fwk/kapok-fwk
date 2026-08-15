using Xunit;

namespace Kapok.Core.UnitTest.Data;

public class ChangeTrackerTest
{
    private class Entity
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// An entity type which overrides Equals/GetHashCode with value semantics, used to verify that
    /// ChangeTracker tracks entities by reference identity and not by value equality.
    /// </summary>
    private class ValueEqualityEntity
    {
        public int Id { get; set; }

        public override bool Equals(object? obj) => obj is ValueEqualityEntity other && other.Id == Id;
        public override int GetHashCode() => Id;
    }

    [Fact]
    public void Add_ThenGet_ReturnsTheSameTrackingObject()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entity = new Entity { Id = 1 };

        var added = changeTracker.Add(entity);
        var retrieved = changeTracker.Get(entity);

        Assert.Same(added, retrieved);
        Assert.Same(entity, retrieved?.Entity);
    }

    [Fact]
    public void Get_ForUntrackedEntity_ReturnsNull()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entity = new Entity { Id = 1 };

        Assert.Null(changeTracker.Get(entity));
    }

    [Fact]
    public void Get_UsesReferenceIdentity_NotValueEquality()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();

        // two distinct instances which are value-equal to each other
        var entityA = new ValueEqualityEntity { Id = 1 };
        var entityB = new ValueEqualityEntity { Id = 1 };
        Assert.Equal(entityA, entityB); // sanity check: the type really does have value equality

        changeTracker.Add(entityA);

        // entityB was never added, so it must not be found, even though it is "Equals" to entityA
        Assert.Null(changeTracker.Get(entityB));
        Assert.NotNull(changeTracker.Get(entityA));
    }

    [Fact]
    public void Detach_RemovesEntity_SoGetReturnsNullAfterwards()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entity = new Entity { Id = 1 };
        var tracking = changeTracker.Add(entity);

        changeTracker.Detach(tracking);

        Assert.Null(changeTracker.Get(entity));
        Assert.Empty(changeTracker);
    }

    [Fact]
    public void Add_CalledTwiceForSameReference_DoesNotThrow_AndOverwritesTheEntry()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entity = new Entity { Id = 1 };

        var first = changeTracker.Add(entity);
        var second = changeTracker.Add(entity);

        Assert.Same(second, changeTracker.Get(entity));
        Assert.Single(changeTracker);
    }

    [Fact]
    public void Clear_RemovesAllTrackedEntities()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        changeTracker.Add(new Entity { Id = 1 });
        changeTracker.Add(new Entity { Id = 2 });

        changeTracker.Clear();

        Assert.Empty(changeTracker);
    }

    [Fact]
    public void AnyChangesOutstanding_ReflectsTrackedStates()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entity = new Entity { Id = 1 };
        var tracking = changeTracker.Add(entity);

        Assert.False(changeTracker.AnyChangesOutstanding());

        tracking.State = Kapok.Data.ChangeTrackingState.Updated;

        Assert.True(changeTracker.AnyChangesOutstanding());
    }

    [Fact]
    public void GetEnumerator_EnumeratesEveryTrackedEntity()
    {
        var changeTracker = new Kapok.Data.ChangeTracker();
        var entityA = new Entity { Id = 1 };
        var entityB = new Entity { Id = 2 };
        changeTracker.Add(entityA);
        changeTracker.Add(entityB);

        var trackedEntities = changeTracker.Select(ct => ct.Entity).ToList();

        Assert.Equal(2, trackedEntities.Count);
        Assert.Contains(entityA, trackedEntities);
        Assert.Contains(entityB, trackedEntities);
    }
}
