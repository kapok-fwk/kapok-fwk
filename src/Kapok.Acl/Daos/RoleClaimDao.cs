using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class RoleClaimDao : Dao<RoleClaim>, IRoleClaimDao
{
    public RoleClaimDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}