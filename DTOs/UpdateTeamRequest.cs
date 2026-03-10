using System.ComponentModel.DataAnnotations;

namespace cursor_dotnet_test.DTOs;

public class UpdateTeamRequest
{
    [Required(ErrorMessage = "TeamName is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "TeamName must be between 1 and 200 characters.")]
    public string TeamName { get; set; } = string.Empty;

    [Required(ErrorMessage = "ManagerName is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "ManagerName must be between 1 and 200 characters.")]
    public string ManagerName { get; set; } = string.Empty;
}
