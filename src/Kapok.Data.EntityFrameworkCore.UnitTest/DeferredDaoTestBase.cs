using System.Data;
using System.Diagnostics;
using Kapok.BusinessLayer;
using Microsoft.EntityFrameworkCore;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

public abstract class DeferredDaoTestBase : IDisposable
{
    private readonly IDataDomain _dataDomain;

    // A connection which is kept alive as long as the unit test runs.
    // We use by default a SQLite shared memory database. To ensure
    // that the data is not removed we need to keep a connection alive while
    // the database is used. See also:
    // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases
    private readonly IDbConnection _cacheDbConnection;
    private readonly EFCoreDataDomainScope _cacheScope;
    
    public DeferredDaoTestBase(IDataDomain? dataDomain = null)
    {
        Kapok.Data.DataDomain.DefaultDaoType = typeof(DeferredDao<>);
        _dataDomain = dataDomain ?? InitializeDataDomain();

        // oping a connection to SQLite
        _cacheScope = (EFCoreDataDomainScope)_dataDomain.CreateScope();
        Debug.Assert(_cacheScope.DbContext != null);
        _cacheDbConnection = _cacheScope.DbContext.Database.GetDbConnection();
        _cacheDbConnection.Open();

        // ensures that the test database is clean created. 
        // we drop the DB first because some database providers might have a
        // test database not cleaned in an inconsistent state.
        _cacheScope.DbContext.Database.EnsureDeleted();
        _cacheScope.DbContext.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        _cacheDbConnection.Close();
        _cacheScope.Dispose();
    }
    
    public IDataDomain InitializeDataDomain(DbContextOptions? dbContextOptions = null)
    {
        if (dbContextOptions == null)
        {
            var contextOptionBuilder = new DbContextOptionsBuilder();

            // testing against SQLite database
            // Requires NuGet package Microsoft.EntityFrameworkCore.Sqlite
            contextOptionBuilder.UseSqlite($"Data Source={nameof(DeferredDaoSampleModelTests)};Mode=Memory;Cache=Shared");

            // testing against a local SQL Server database
            // Requires NuGet package Microsoft.EntityFrameworkCore.SqlServer
            //contextOptionBuilder.UseSqlServer(@"Server=(local);Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0",
            //    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            dbContextOptions = contextOptionBuilder.Options;
        }

        InitiateModule();

        return new EFCoreDataDomain(dbContextOptions);
    }

    protected abstract void InitiateModule();

    public IDataDomain DataDomain => _dataDomain;
}