using Xunit;

namespace Kapok.View.UnitTest;

public class PropertyViewTest
{
    internal static void ValidatePropertyId(PropertyView propertyViewId)
    {
        Assert.NotNull(propertyViewId.PropertyInfo);
        Assert.Equal(typeof(SampleEntity), propertyViewId.DeclaringType);
        Assert.Equal(typeof(SampleEntity), propertyViewId.PropertyInfo?.DeclaringType);
        Assert.NotNull(propertyViewId.Name);
        Assert.Equal(nameof(SampleEntity.Id), propertyViewId.Name);
    }
    
    internal static void ValidatePropertyName(PropertyView propertyViewName)
    {
        Assert.NotNull(propertyViewName.PropertyInfo);
        Assert.Equal(typeof(SampleEntity), propertyViewName.DeclaringType);
        Assert.Equal(typeof(SampleEntity), propertyViewName.PropertyInfo?.DeclaringType);
        Assert.NotNull(propertyViewName.Name);
        Assert.Equal(nameof(SampleEntity.Name), propertyViewName.Name);
    }
    
    [Fact]
    public void PropertyInfoTest()
    {
        var propertyViewId = new PropertyView(typeof(SampleEntity).GetProperty(nameof(SampleEntity.Id)));

        ValidatePropertyId(propertyViewId);

        var propertyViewName = new PropertyView(typeof(SampleEntity).GetProperty(nameof(SampleEntity.Name)));

        ValidatePropertyName(propertyViewName);
    }

    [Fact]
    public void PropertyNameTest()
    {
        var propertyViewId = new PropertyView(nameof(SampleEntity.Id));
        
        Assert.Null(propertyViewId.PropertyInfo);
        Assert.Null(propertyViewId.DeclaringType);
        Assert.NotNull(propertyViewId.Name);
        Assert.Equal(nameof(SampleEntity.Id), propertyViewId.Name);

        var propertyViewName = new PropertyView(nameof(SampleEntity.Name));
        
        Assert.Null(propertyViewName.PropertyInfo);
        Assert.Null(propertyViewId.DeclaringType);
        Assert.NotNull(propertyViewName.Name);
        Assert.Equal(nameof(SampleEntity.Name), propertyViewName.Name);
        
        // set declaring type
        propertyViewId.DeclaringType = typeof(SampleEntity);
        propertyViewName.DeclaringType = typeof(SampleEntity);
        
        // expect now that PropertyInfo and DeclaringType is set correctly
        Assert.NotNull(propertyViewId.PropertyInfo);
        Assert.Equal(typeof(SampleEntity), propertyViewId.DeclaringType);
        Assert.Equal(typeof(SampleEntity), propertyViewId.PropertyInfo?.DeclaringType);
        Assert.Equal(nameof(SampleEntity.Id), propertyViewId.Name);
        
        Assert.NotNull(propertyViewName.PropertyInfo);
        Assert.Equal(typeof(SampleEntity), propertyViewName.DeclaringType);
        Assert.Equal(typeof(SampleEntity), propertyViewName.PropertyInfo?.DeclaringType);
        Assert.Equal(nameof(SampleEntity.Name), propertyViewName.Name);
    }
}