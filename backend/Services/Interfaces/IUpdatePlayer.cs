namespace cursor_dotnet_test.Services.Interfaces;

public interface IUpdatePlayer
{
    Task UpdatePlayer(Guid playerId, string playerName, string playerPosition, int playerAge, Guid teamId);
}
