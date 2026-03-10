namespace cursor_dotnet_test.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
