using cursor_dotnet_test.Persistence.DataModels;

namespace cursor_dotnet_test.Persistence.Repositories;

public interface IPlayerRepository
{
    Task<PlayerDataModel?> GetByIdAsync(Guid playerId);
    Task<List<PlayerDataModel>> GetAllByTeamAsync(Guid teamId);
    Task<PlayerDataModel> CreateAsync(PlayerDataModel player);
    Task UpdateAsync(Guid playerId, string playerName, string playerPosition, int playerAge, Guid teamId);
    Task DeleteAsync(Guid playerId);
}
