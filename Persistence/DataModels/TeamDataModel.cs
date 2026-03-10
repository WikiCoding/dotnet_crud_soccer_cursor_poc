using System.ComponentModel.DataAnnotations;

namespace cursor_dotnet_test.Persistence.DataModels;

public class TeamDataModel
{
    [Key]
    public Guid TeamId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ManagerName { get; set; } = string.Empty;

    [ConcurrencyCheck]
    public int Version { get; set; }
}
