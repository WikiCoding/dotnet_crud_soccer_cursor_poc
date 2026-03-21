namespace cursor_dotnet_test.Domain;

public class Team
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
}
