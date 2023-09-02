using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.LoginProvider;

namespace Kapok.Acl.DataModel;

[Table(nameof(LoginProvider), Schema = "System")]
public class LoginProvider : EditableEntityBase
{
    private byte[]? _rowVersion;
    private Guid _id;
    private string? _name;
    private string? _authenticationServiceClass;
    private JsonObject? _configuration;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Key]
    [Browsable(false)]
    [Display(Name = nameof(Id), ResourceType = typeof(Res))]
    public Guid Id
    {
        get => _id;
        set => SetValidateProperty(ref _id, value);
    }
    
    [Display(Name = nameof(Name), ResourceType = typeof(Res))]
    [LookupColumn]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    [Display(Name = nameof(AuthenticationServiceClass), ResourceType = typeof(Res))]
    public string? AuthenticationServiceClass
    {
        get => _authenticationServiceClass;
        set => SetProperty(ref _authenticationServiceClass, value);
    }

    public JsonObject? Configuration
    {
        get => _configuration;
        set => SetProperty(ref _configuration, value);
    }
}