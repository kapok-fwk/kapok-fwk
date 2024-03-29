﻿using Kapok.Acl.DataModel;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Acl.BusinessLayer;

public sealed class RoleClaimDao : Dao<RoleClaim>, IRoleClaimDao
{
    public RoleClaimDao(IDataDomainScope dataDomainScope, IRepository<RoleClaim> repository, bool isReadOnly = false)
        : base(dataDomainScope, repository, isReadOnly)
    {
    }
}