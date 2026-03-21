using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface IGetPlayersByTeam
{
    Task<List<Player>> GetPlayersByTeam(Guid teamId);
}
