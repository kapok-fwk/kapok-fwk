using System.ComponentModel.DataAnnotations;
using Res = Kapok.Core.Resources.BusinessLayer.MessageSeverity;
// ReSharper disable UnusedMember.Global

namespace Kapok.Core;

public enum MessageSeverity
{
    [Display(Name = "Debug", ResourceType = typeof(Res))]
    Debug,

    [Display(Name = "Info", ResourceType = typeof(Res))]
    Info,

    [Display(Name = "Warning", ResourceType = typeof(Res))]
    Warning,

    [Display(Name = "Error", ResourceType = typeof(Res))]
    Error
}