using Kapok.Acl.DataModel;
using Kapok.Data;
using Kapok.Data.EntityFrameworkCore.UnitTest;
using Kapok.Module;
using Newtonsoft.Json;

namespace Kapok.Acl.UnitTest;

public class SerializationTest : DeferredDaoTestBase
{
    protected override void InitiateModule()
    {
        ModuleEngine.InitiateModule(typeof(AclModule));
    }

    private static async Task SeedUser(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var userDao = scope.GetDao<User>();

        var newUser = userDao.New();
        newUser.Id = new Guid("8abcd652-cc76-442d-97b7-05e23b164e63");
        newUser.UserName = "John Doe";
        await userDao.CreateAsync(newUser);

        await scope.SaveAsync();

        Assert.Equal(1, userDao.AsQueryable().Count());
    }

    private static async Task SeedUserLogin(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var userLoginDao = scope.GetDao<UserLogin>();

        var newUserLogin = userLoginDao.New();
        newUserLogin.UserId = new Guid("8abcd652-cc76-442d-97b7-05e23b164e63");
        newUserLogin.LoginProvider = "TestProvider";
        newUserLogin.ProviderKey = "abcdef123456";
        await userLoginDao.CreateAsync(newUserLogin);

        await scope.SaveAsync();

        Assert.Equal(1, userLoginDao.AsQueryable().Count());
    }

    /// <summary>
    /// Tests if the database seeding works as expected
    /// </summary>
    [Fact]
    public async void SerializeUserTest()
    {
        await SeedUser(DataDomain);

        using var scope = DataDomain.CreateScope();
        var userDao = scope.GetDao<User>();

        // text json serialization of a user
        var user = userDao.AsQueryable().First();

        var serialized = JsonConvert.SerializeObject(user, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            NullValueHandling = NullValueHandling.Ignore
        });
        
        Assert.Equal("{\"Id\":\"8abcd652-cc76-442d-97b7-05e23b164e63\",\"UserName\":\"John Doe\"}", serialized);
    }

    /// <summary>
    /// Tests if the database seeding works as expected
    /// </summary>
    [Fact]
    public async void SerializeUserLoginTest()
    {
        await SeedUser(DataDomain);
        await SeedUserLogin(DataDomain);

        using var scope = DataDomain.CreateScope();
        var userLoginDao = scope.GetDao<UserLogin>();

        // text json serialization of a user
        var userLogin = userLoginDao.AsQueryable().First();

        var serialized = JsonConvert.SerializeObject(userLogin, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        Assert.Equal("{\"UserId\":\"8abcd652-cc76-442d-97b7-05e23b164e63\",\"LoginProvider\":\"TestProvider\",\"ProviderKey\":\"abcdef123456\"}", serialized);
    }
}