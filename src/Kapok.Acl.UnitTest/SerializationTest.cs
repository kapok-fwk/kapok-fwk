using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Kapok.Acl.DataModel;
using Kapok.Data;
using Kapok.Data.EntityFrameworkCore.UnitTest;
using Kapok.Module;

namespace Kapok.Acl.UnitTest;

public class SerializationTest : DeferredEntityServiceTestBase
{
    protected override void InitiateModule()
    {
        ModuleEngine.InitiateModule(typeof(AclModule));
    }

    private static async Task SeedUser(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var userService = scope.GetEntityService<User>();

        var newUser = userService.New();
        newUser.Id = new Guid("8abcd652-cc76-442d-97b7-05e23b164e63");
        newUser.UserName = "John Doe";
        await userService.CreateAsync(newUser);

        await scope.SaveAsync();

        Assert.Equal(1, userService.AsQueryable().Count());
    }

    private static async Task SeedUserLogin(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var userLoginService = scope.GetEntityService<UserLogin>();

        var newUserLogin = userLoginService.New();
        newUserLogin.UserId = new Guid("8abcd652-cc76-442d-97b7-05e23b164e63");
        newUserLogin.LoginProvider = "TestProvider";
        newUserLogin.ProviderKey = "abcdef123456";
        await userLoginService.CreateAsync(newUserLogin);

        await scope.SaveAsync();

        Assert.Equal(1, userLoginService.AsQueryable().Count());
    }

    private static async Task SeedLoginProvider(IDataDomain dataDomain)
    {
        using var scope = dataDomain.CreateScope();
        var loginProviderService = scope.GetEntityService<LoginProvider>();

        var newLoginProvider = loginProviderService.New();
        newLoginProvider.Id = new Guid("ef794e82-20dc-4ea0-a7f2-abf7675e3aef");
        newLoginProvider.Name = "MSAL";
        newLoginProvider.AuthenticationServiceClass = "<tbd>";
        newLoginProvider.Configuration = new JsonObject
        {
            { "Instance", "https://login.microsoftonline.com/{0}/v2.0" },
            { "ClientId", "" },
            { "TenantId", "common" }
        };
        await loginProviderService.CreateAsync(newLoginProvider);
        await scope.SaveAsync();

        Assert.Equal(1, loginProviderService.AsQueryable().Count());
    }

    [Fact]
    public async void SerializeLoginProviderTest()
    {
        await SeedLoginProvider(DataDomain);
        
        using var scope = DataDomain.CreateScope();
        var loginProviderService = scope.GetEntityService<LoginProvider>();

        // text json serialization of a user
        var loginProvider = loginProviderService.AsQueryable().First();

        var serialized = JsonSerializer.Serialize(loginProvider, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        Assert.Equal("{\"Id\":\"ef794e82-20dc-4ea0-a7f2-abf7675e3aef\",\"Name\":\"MSAL\",\"AuthenticationServiceClass\":\"\\u003Ctbd\\u003E\",\"Configuration\":{\"Instance\":\"https://login.microsoftonline.com/{0}/v2.0\",\"ClientId\":\"\",\"TenantId\":\"common\"}}", serialized);
    }

    /// <summary>
    /// Tests if the database seeding works as expected
    /// </summary>
    [Fact]
    public async void SerializeUserTest()
    {
        await SeedUser(DataDomain);

        using var scope = DataDomain.CreateScope();
        var userService = scope.GetEntityService<User>();

        // text json serialization of a user
        var user = userService.AsQueryable().First();

        var serialized = JsonSerializer.Serialize(user, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        var userLoginService = scope.GetEntityService<UserLogin>();

        // text json serialization of a user
        var userLogin = userLoginService.AsQueryable().First();

        var serialized = JsonSerializer.Serialize(userLogin, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        Assert.Equal("{\"UserId\":\"8abcd652-cc76-442d-97b7-05e23b164e63\",\"LoginProvider\":\"TestProvider\",\"ProviderKey\":\"abcdef123456\"}", serialized);
    }
}