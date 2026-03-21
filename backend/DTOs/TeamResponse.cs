namespace cursor_dotnet_test.DTOs;

public class TeamResponse
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public int Version { get; set; }
}
