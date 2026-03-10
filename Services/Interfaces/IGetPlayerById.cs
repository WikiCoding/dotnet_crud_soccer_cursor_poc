using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface IGetPlayerById
{
    Task<Player> GetPlayerById(Guid playerId);
}
