using System.ComponentModel.DataAnnotations;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.LoginProvider;

namespace Kapok.Acl.DataModel;

// NOTE: this is (current) considered to be a 'virtual entity'. Therefore it is not added to the AclModule
public class LoginProvider : EntityBase
{
    private string? _name;

    [Display(Name = "Name", ResourceType = typeof(Res))]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _authenticationServiceClass;

    [Display(Name = "AuthenticationServiceClass", ResourceType = typeof(Res))]
    public string? AuthenticationServiceClass
    {
        get => _authenticationServiceClass;
        set => SetProperty(ref _authenticationServiceClass, value);
    }
}