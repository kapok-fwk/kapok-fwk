using Kapok.Data;
using Xunit;

namespace Kapok.Core.UnitTest.Data;

public class DtoMapperTest
{
    private class Entity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime? CreatedAt { get; set; }

        // read-only computed property: must never be written to by the mapper
        public string ComputedLabel => $"{Id}-{Name}";
    }

    [Fact]
    public void GetProperties_ReturnsOnlyReadWriteProperties()
    {
        var properties = DtoMapper.GetProperties(typeof(Entity));

        Assert.Contains(properties, p => p.Name == nameof(Entity.Id));
        Assert.Contains(properties, p => p.Name == nameof(Entity.Name));
        Assert.Contains(properties, p => p.Name == nameof(Entity.Amount));
        Assert.Contains(properties, p => p.Name == nameof(Entity.CreatedAt));
        Assert.DoesNotContain(properties, p => p.Name == nameof(Entity.ComputedLabel));
    }

    [Fact]
    public void GetProperties_CalledTwiceForSameType_ReturnsTheSameCachedArray()
    {
        // pins down that repeated calls are served from the per-Type cache (the point of this PR)
        // instead of re-running reflection (Type.GetProperties) on every call.
        var first = DtoMapper.GetProperties(typeof(Entity));
        var second = DtoMapper.GetProperties(typeof(Entity));

        Assert.Same(first, second);
    }

    [Fact]
    public void Map_TypeObjectObject_CopiesAllReadWriteProperties()
    {
        var source = new Entity { Id = 1, Name = "Alpha", Amount = 12.5m, CreatedAt = new DateTime(2024, 1, 1) };
        var destination = new Entity();

        DtoMapper.Map(typeof(Entity), source, destination);

        Assert.Equal(source.Id, destination.Id);
        Assert.Equal(source.Name, destination.Name);
        Assert.Equal(source.Amount, destination.Amount);
        Assert.Equal(source.CreatedAt, destination.CreatedAt);
    }

    [Fact]
    public void Map_TypeObject_ReturnsNewClonedInstance()
    {
        var source = new Entity { Id = 2, Name = "Beta", Amount = 3.25m };

        var clonedObj = DtoMapper.Map(typeof(Entity), source);

        var cloned = Assert.IsType<Entity>(clonedObj);
        Assert.NotSame(source, cloned);
        Assert.Equal(source.Id, cloned.Id);
        Assert.Equal(source.Name, cloned.Name);
        Assert.Equal(source.Amount, cloned.Amount);
    }

    [Fact]
    public void Map_PropertyInfoArray_CopiesOnlyTheListedProperties()
    {
        var source = new Entity { Id = 3, Name = "Gamma", Amount = 7m };
        var destination = new Entity { Id = 999, Name = "unchanged", Amount = 999m };

        var idOnly = new[] { typeof(Entity).GetProperty(nameof(Entity.Id))! };
        DtoMapper.Map(idOnly, source, destination);

        Assert.Equal(3, destination.Id);
        Assert.Equal("unchanged", destination.Name); // not in the property list -> untouched
        Assert.Equal(999m, destination.Amount);       // not in the property list -> untouched
    }

    [Fact]
    public void Map_HandlesNullReferenceTypeValues()
    {
        var source = new Entity { Id = 4, Name = null };
        var destination = new Entity { Name = "will be overwritten" };

        DtoMapper.Map(typeof(Entity), source, destination);

        Assert.Null(destination.Name);
    }

    [Fact]
    public void GenericDtoMapper_Map_ClonesInstance()
    {
        var source = new Entity { Id = 5, Name = "Delta", Amount = 1.5m };
        var mapper = new DtoMapper<Entity>();

        var cloned = mapper.Map(source);

        Assert.NotSame(source, cloned);
        Assert.Equal(source.Id, cloned.Id);
        Assert.Equal(source.Name, cloned.Name);
        Assert.Equal(source.Amount, cloned.Amount);
    }
}
