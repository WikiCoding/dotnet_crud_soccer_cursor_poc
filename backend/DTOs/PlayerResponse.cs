namespace cursor_dotnet_test.DTOs;

public class PlayerResponse
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerPosition { get; set; } = string.Empty;
    public int PlayerAge { get; set; }
    public Guid TeamId { get; set; }
    public int Version { get; set; }
}
