using System.ComponentModel.DataAnnotations;

namespace cursor_dotnet_test.DTOs;

public class UpdatePlayerRequest
{
    [Required(ErrorMessage = "PlayerName is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "PlayerName must be between 1 and 200 characters.")]
    public string PlayerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "PlayerPosition is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "PlayerPosition must be between 1 and 100 characters.")]
    public string PlayerPosition { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "PlayerAge must be between 1 and 100.")]
    public int PlayerAge { get; set; }

    [Required(ErrorMessage = "TeamId is required.")]
    public Guid TeamId { get; set; }
}
