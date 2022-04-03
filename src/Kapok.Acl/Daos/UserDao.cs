using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class UserDao : Dao<User>, IUserDao
{
    public UserDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}