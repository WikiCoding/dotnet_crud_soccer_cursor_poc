using cursor_dotnet_test.Domain;
using cursor_dotnet_test.Persistence.DataModels;
using cursor_dotnet_test.Persistence.Repositories;
using cursor_dotnet_test.Services.Interfaces;

namespace cursor_dotnet_test.Services;

public class PlayersService : ICreatePlayer, IGetPlayerById, IGetPlayersByTeam, IUpdatePlayer, IDeletePlayer
{
    private readonly IPlayerRepository _repository;
    private readonly ILogger<PlayersService> _logger;

    public PlayersService(IPlayerRepository repository, ILogger<PlayersService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Player>> GetPlayersByTeam(Guid teamId)
    {
        _logger.LogInformation("Retrieving all players for team '{TeamId}'", teamId);

        var dataModels = await _repository.GetAllByTeamAsync(teamId);

        return dataModels.Select(ToPlayer).ToList();
    }

    public async Task<Player> CreatePlayer(string playerName, string playerPosition, int playerAge, Guid teamId)
    {
        _logger.LogInformation("Creating player '{PlayerName}' for team '{TeamId}'", playerName, teamId);

        var dataModel = new PlayerDataModel
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = playerName,
            PlayerPosition = playerPosition,
            PlayerAge = playerAge,
            TeamId = teamId,
            Version = 1
        };

        var created = await _repository.CreateAsync(dataModel);

        _logger.LogInformation("Player '{PlayerId}' created successfully", created.PlayerId);
        return ToPlayer(created);
    }

    public async Task<Player> GetPlayerById(Guid playerId)
    {
        _logger.LogInformation("Retrieving player '{PlayerId}'", playerId);

        var dataModel = await _repository.GetByIdAsync(playerId)
            ?? throw new KeyNotFoundException($"Player with id '{playerId}' not found.");

        return ToPlayer(dataModel);
    }

    public async Task UpdatePlayer(Guid playerId, string playerName, string playerPosition, int playerAge, Guid teamId)
    {
        _logger.LogInformation("Updating player '{PlayerId}'", playerId);
        await _repository.UpdateAsync(playerId, playerName, playerPosition, playerAge, teamId);
        _logger.LogInformation("Player '{PlayerId}' updated successfully", playerId);
    }

    public async Task DeletePlayer(Guid playerId)
    {
        _logger.LogInformation("Deleting player '{PlayerId}'", playerId);
        await _repository.DeleteAsync(playerId);
        _logger.LogInformation("Player '{PlayerId}' deleted successfully", playerId);
    }

    private static Player ToPlayer(PlayerDataModel model) => new()
    {
        PlayerId = model.PlayerId,
        PlayerName = model.PlayerName,
        PlayerPosition = model.PlayerPosition,
        PlayerAge = model.PlayerAge,
        TeamId = model.TeamId
    };
}
