using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class UserLoginDao : Dao<UserLogin>, IUserLoginDao
{
    public UserLoginDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}