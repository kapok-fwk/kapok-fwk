using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class UserRoleDao : Dao<UserRole>, IUserRoleDao
{
    public UserRoleDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}