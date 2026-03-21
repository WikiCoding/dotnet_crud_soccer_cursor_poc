using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface ICreatePlayer
{
    Task<Player> CreatePlayer(string playerName, string playerPosition, int playerAge, Guid teamId);
}
