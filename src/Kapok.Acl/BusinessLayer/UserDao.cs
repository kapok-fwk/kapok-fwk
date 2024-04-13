using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class UserService : EntityService<User>, IUserService
{
    public UserService(IDataDomainScope dataDomainScope, IRepository<User> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}