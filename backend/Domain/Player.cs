namespace cursor_dotnet_test.Domain;

public class Player
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerPosition { get; set; } = string.Empty;
    public int PlayerAge { get; set; }
    public Guid TeamId { get; set; }
}
