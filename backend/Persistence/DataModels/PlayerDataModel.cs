using System.ComponentModel.DataAnnotations;

namespace cursor_dotnet_test.Persistence.DataModels;

public class PlayerDataModel
{
    [Key]
    public Guid PlayerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PlayerName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PlayerPosition { get; set; } = string.Empty;

    public int PlayerAge { get; set; }

    public Guid TeamId { get; set; }

    [ConcurrencyCheck]
    public int Version { get; set; }
}
