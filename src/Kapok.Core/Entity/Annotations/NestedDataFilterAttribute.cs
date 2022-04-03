using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.Entity;

[AttributeUsage(AttributeTargets.Property)]
public class NestedDataFilterAttribute : NotMappedAttribute
{
}