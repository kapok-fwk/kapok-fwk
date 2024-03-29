﻿using System.ComponentModel.DataAnnotations;
using Res = Kapok.Acl.Resources.ClaimType;

namespace Kapok.Acl;

// note: max length should be 10 of an entry (this is used in the database)
public enum ClaimType
{
    [Display(Name = nameof(System), ResourceType = typeof(Res))]
    System = 1,

    [Display(Name = nameof(DataRead), ResourceType = typeof(Res))]
    DataRead = 2,

    [Display(Name = nameof(DataWrite), ResourceType = typeof(Res))]
    DataWrite = 3,

    /// <summary>
    /// Grants access to a specific data partition.
    ///
    /// The claim value is defined as `PartitionName:PartitionKey`.
    /// </summary>
    [Display(Name = nameof(Partition), ResourceType = typeof(Res))]
    Partition = 4,

    // Enum 5 is not used yet

    [Display(Name = nameof(Function), ResourceType = typeof(Res))]
    Function = 6,

    [Display(Name = nameof(Page), ResourceType = typeof(Res))]
    Page = 7
}