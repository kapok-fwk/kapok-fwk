namespace Kapok.Core.UnitTest.DataModel;

/// <summary>
/// A entity connected to a tenant
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The tenant id property.
    /// </summary>
    public long TenantId { get; set; }
}