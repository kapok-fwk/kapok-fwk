using Kapok.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.Core.DataModel;

[Table("MigrationsHistory", Schema = "System")]
public class ModuleMigrationsHistory : EntityBase
{
    static ModuleMigrationsHistory()
    {
        RegisterModel<ModuleMigrationsHistory>(entity =>
        {
            entity.SetPrimaryKey(nameof(ModuleName), nameof(MigrationId));
        });
    }

    private string? _moduleName;
    private string? _migrationId;
    private string? _productVersion;

    [Required]
    [StringLength(150)]
    public string? ModuleName
    {
        get => _moduleName;
        set => SetProperty(ref _moduleName, value);
    }

    [Required]
    [StringLength(150)]
    public string? MigrationId
    {
        get => _migrationId;
        set => SetProperty(ref _migrationId, value);
    }

    [Required]
    [StringLength(32)]
    public string? ProductVersion
    {
        get => _productVersion;
        set => SetProperty(ref _productVersion, value);
    }
}