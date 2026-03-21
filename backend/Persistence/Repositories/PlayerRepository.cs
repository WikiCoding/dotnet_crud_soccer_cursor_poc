using cursor_dotnet_test.Persistence.DataModels;
using Microsoft.EntityFrameworkCore;

namespace cursor_dotnet_test.Persistence.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly SoccerDbContext _context;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<PlayerRepository> _logger;

    private const string PlayerKeyPrefix = "player:";
    private const string PlayersByTeamPrefix = "players:team:";

    public PlayerRepository(SoccerDbContext context, IRedisCacheService cache, ILogger<PlayerRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PlayerDataModel?> GetByIdAsync(Guid playerId)
    {
        var cacheKey = $"{PlayerKeyPrefix}{playerId}";
        var cached = await _cache.GetAsync<PlayerDataModel>(cacheKey);
        if (cached is not null)
            return cached;

        _logger.LogDebug("Cache MISS — fetching player {PlayerId} from database", playerId);
        var player = await _context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.PlayerId == playerId);

        if (player is not null)
            await _cache.SetAsync(cacheKey, player);

        return player;
    }

    public async Task<List<PlayerDataModel>> GetAllByTeamAsync(Guid teamId)
    {
        var cacheKey = $"{PlayersByTeamPrefix}{teamId}";
        var cached = await _cache.GetAsync<List<PlayerDataModel>>(cacheKey);
        if (cached is not null)
            return cached;

        _logger.LogDebug("Cache MISS — fetching all players for team {TeamId}", teamId);

        var players = await _context.Players
            .AsNoTracking()
            .Where(p => p.TeamId == teamId)
            .OrderBy(p => p.PlayerName)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, players);

        _logger.LogDebug("Found {Count} players for team {TeamId}", players.Count, teamId);
        return players;
    }

    public async Task<PlayerDataModel> CreateAsync(PlayerDataModel player)
    {
        _logger.LogDebug("Inserting player {PlayerId} into database", player.PlayerId);
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        await _cache.RemoveAsync($"{PlayersByTeamPrefix}{player.TeamId}");

        _logger.LogInformation("Player {PlayerId} persisted successfully", player.PlayerId);
        return player;
    }

    public async Task UpdateAsync(Guid playerId, string playerName, string playerPosition, int playerAge, Guid teamId)
    {
        _logger.LogDebug("Updating player {PlayerId}", playerId);

        var affectedRows = await _context.Players
            .Where(p => p.PlayerId == playerId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.PlayerName, playerName)
                .SetProperty(p => p.PlayerPosition, playerPosition)
                .SetProperty(p => p.PlayerAge, playerAge)
                .SetProperty(p => p.TeamId, teamId)
                .SetProperty(p => p.Version, p => p.Version + 1));

        if (affectedRows == 0)
            throw new KeyNotFoundException($"Player with id '{playerId}' not found.");

        await _cache.RemoveAsync($"{PlayerKeyPrefix}{playerId}");
        await _cache.RemoveByPrefixAsync(PlayersByTeamPrefix);

        _logger.LogInformation("Player {PlayerId} updated ({AffectedRows} row(s) affected)", playerId, affectedRows);
    }

    public async Task DeleteAsync(Guid playerId)
    {
        _logger.LogDebug("Deleting player {PlayerId}", playerId);

        var affectedRows = await _context.Players
            .Where(p => p.PlayerId == playerId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
            throw new KeyNotFoundException($"Player with id '{playerId}' not found.");

        await _cache.RemoveAsync($"{PlayerKeyPrefix}{playerId}");
        await _cache.RemoveByPrefixAsync(PlayersByTeamPrefix);

        _logger.LogInformation("Player {PlayerId} deleted ({AffectedRows} row(s) affected)", playerId, affectedRows);
    }
}
