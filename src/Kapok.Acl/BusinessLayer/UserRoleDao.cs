using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class UserRoleService : EntityService<UserRole>, IUserRoleService
{
    public UserRoleService(IDataDomainScope dataDomainScope, IRepository<UserRole> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}