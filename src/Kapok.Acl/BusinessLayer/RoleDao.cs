using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class RoleService : EntityService<Role>, IRoleService
{
    public RoleService(IDataDomainScope dataDomainScope, IRepository<Role> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}