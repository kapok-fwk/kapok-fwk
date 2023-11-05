using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class UserRoleDao : Dao<UserRole>, IUserRoleDao
{
    public UserRoleDao(IDataDomainScope dataDomainScope, IRepository<UserRole> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}