using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class UserLoginDao : Dao<UserLogin>, IUserLoginDao
{
    public UserLoginDao(IDataDomainScope dataDomainScope)
        : base(dataDomainScope)
    {
    }
}