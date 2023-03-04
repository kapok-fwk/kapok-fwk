using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class RoleClaimDao : Dao<RoleClaim>, IRoleClaimDao
{
    public RoleClaimDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}