using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class UserLoginDao : Dao<UserLogin>, IUserLoginDao
{
    public UserLoginDao(IDataDomainScope dataDomainScope, IRepository<UserLogin> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}