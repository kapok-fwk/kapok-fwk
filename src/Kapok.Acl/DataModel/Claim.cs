using System.ComponentModel.DataAnnotations;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.Claim;

namespace Kapok.Acl.DataModel;

// NOTE: this is (current) considered to be a 'virtual entity'. Therefore it is not added to the AclModule
public class Claim : EntityBase
{
    private ClaimType _type;

    [Display(Name = "Type", ResourceType = typeof(Res))]
    public ClaimType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    private string? _name;

    [Display(Name = "Name", ResourceType = typeof(Res))]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _description;

    [Display(Name = "Description", ResourceType = typeof(Res))]
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
}