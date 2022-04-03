using Kapok.Core;
using Microsoft.EntityFrameworkCore;

namespace Kapok.Data.EntityFrameworkCore
{
    public interface IEntityFrameworkCoreDataDomain : IDataDomain
    {
        DbContext ConstructNewDbContext();
    }
}