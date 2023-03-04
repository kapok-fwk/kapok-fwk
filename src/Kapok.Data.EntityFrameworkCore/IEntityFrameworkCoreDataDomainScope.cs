using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kapok.Data.EntityFrameworkCore;

[Obsolete("This interface allows to use DbContext which is now an internal property!")]
public interface IEntityFrameworkCoreDataDomainScope : IDataDomainScope
{
    DbContext? DbContext { get; }
    IModel? Model { get; }
}