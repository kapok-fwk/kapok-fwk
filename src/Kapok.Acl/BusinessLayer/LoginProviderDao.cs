using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class LoginProviderDao : Dao<LoginProvider>, ILoginProviderDao
{
    public LoginProviderDao(IDataDomainScope dataDomainScope, IRepository<LoginProvider> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}