using System.ComponentModel.DataAnnotations;
using Res = Kapok.Acl.Resources.ClaimType;

namespace Kapok.Acl;

// note: max length should be 10 of an entry (this is used in the database)
public enum ClaimType
{
    [Display(Name = "System", ResourceType = typeof(Res))]
    System = 1,

    [Display(Name = "DataRead", ResourceType = typeof(Res))]
    DataRead = 2,

    [Display(Name = "DataWrite", ResourceType = typeof(Res))]
    DataWrite = 3,
        
    /* TODO rethink architecture if that is necessary
    [Display(Name = "TableCreate", ResourceType = typeof(Res))]
    TableCreate = 4,

    [Display(Name = "TableDelete", ResourceType = typeof(Res))]
    TableDelete = 5,*/

    [Display(Name = "Function", ResourceType = typeof(Res))]
    Function = 6,
        
    [Display(Name = "Page", ResourceType = typeof(Res))]
    Page = 7
}