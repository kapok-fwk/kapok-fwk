using Xunit;

namespace Kapok.View.UnitTest;

public class ColumnPropertyViewTest
{
    internal static void ValidatePropertyId(ColumnPropertyView propertyViewId)
    {
        PropertyViewTest.ValidatePropertyId(propertyViewId);
        Assert.True(propertyViewId.IsReadOnly);
    }
    
    internal static void ValidatePropertyName(ColumnPropertyView propertyViewName)
    {
        PropertyViewTest.ValidatePropertyName(propertyViewName);
        Assert.False(propertyViewName.IsReadOnly);
    }
    
    [Fact]
    public void PropertyInfoTest()
    {
        var propertyViewId = new ColumnPropertyView(typeof(SampleEntity).GetProperty(nameof(SampleEntity.Id)));
    
        ValidatePropertyId(propertyViewId);

        var propertyViewName = new ColumnPropertyView(typeof(SampleEntity).GetProperty(nameof(SampleEntity.Name)));
    
        ValidatePropertyName(propertyViewName);
    }

    [Fact]
    public void PropertyNameTest()
    {
        var propertyViewId = new ColumnPropertyView(nameof(SampleEntity.Id));
        
        Assert.Null(propertyViewId.PropertyInfo);
        Assert.Null(propertyViewId.DeclaringType);
        Assert.NotNull(propertyViewId.Name);
        Assert.Equal(nameof(SampleEntity.Id), propertyViewId.Name);

        var propertyViewName = new ColumnPropertyView(nameof(SampleEntity.Name));
        
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