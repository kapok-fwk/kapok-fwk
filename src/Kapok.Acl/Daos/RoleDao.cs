using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class RoleDao : Dao<Role>, IRoleDao
{
    public RoleDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}