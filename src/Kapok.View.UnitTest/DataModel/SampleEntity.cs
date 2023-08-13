using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.View.UnitTest;

public class SampleEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ReadOnly(isReadOnly: true)]
    public Guid? Id { get; set; }

    [Required(AllowEmptyStrings = false)] public string Name { get; set; } = string.Empty;
}