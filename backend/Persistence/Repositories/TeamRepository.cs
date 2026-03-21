using cursor_dotnet_test.Persistence.DataModels;
using Microsoft.EntityFrameworkCore;

namespace cursor_dotnet_test.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly SoccerDbContext _context;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<TeamRepository> _logger;

    private const string TeamKeyPrefix = "team:";
    private const string TeamsAllPrefix = "teams:all:";

    public TeamRepository(SoccerDbContext context, IRedisCacheService cache, ILogger<TeamRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TeamDataModel?> GetByIdAsync(Guid teamId)
    {
        var cacheKey = $"{TeamKeyPrefix}{teamId}";
        var cached = await _cache.GetAsync<TeamDataModel>(cacheKey);
        if (cached is not null)
            return cached;

        _logger.LogDebug("Cache MISS — fetching team {TeamId} from database", teamId);
        var team = await _context.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.TeamId == teamId);

        if (team is not null)
            await _cache.SetAsync(cacheKey, team);

        return team;
    }

    public async Task<(List<TeamDataModel> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string sortBy, bool ascending)
    {
        var cacheKey = $"{TeamsAllPrefix}{page}:{pageSize}:{sortBy}:{ascending}";
        var cached = await _cache.GetAsync<CachedTeamPage>(cacheKey);
        if (cached is not null)
            return (cached.Items, cached.TotalCount);

        _logger.LogDebug("Cache MISS — fetching teams page {Page}, size {PageSize}, sort {SortBy} {Direction}",
            page, pageSize, sortBy, ascending ? "ASC" : "DESC");

        var query = _context.Teams.AsNoTracking();

        query = sortBy.ToLowerInvariant() switch
        {
            "managername" => ascending ? query.OrderBy(t => t.ManagerName) : query.OrderByDescending(t => t.ManagerName),
            _ => ascending ? query.OrderBy(t => t.TeamName) : query.OrderByDescending(t => t.TeamName)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        await _cache.SetAsync(cacheKey, new CachedTeamPage { Items = items, TotalCount = totalCount });

        _logger.LogDebug("Returning {Count} teams out of {Total}", items.Count, totalCount);
        return (items, totalCount);
    }

    public async Task<TeamDataModel> CreateAsync(TeamDataModel team)
    {
        _logger.LogDebug("Inserting team {TeamId} into database", team.TeamId);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        await _cache.RemoveByPrefixAsync(TeamsAllPrefix);

        _logger.LogInformation("Team {TeamId} persisted successfully", team.TeamId);
        return team;
    }

    public async Task UpdateAsync(Guid teamId, string teamName, string managerName)
    {
        _logger.LogDebug("Updating team {TeamId}", teamId);

        var affectedRows = await _context.Teams
            .Where(t => t.TeamId == teamId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.TeamName, teamName)
                .SetProperty(t => t.ManagerName, managerName)
                .SetProperty(t => t.Version, t => t.Version + 1));

        if (affectedRows == 0)
            throw new KeyNotFoundException($"Team with id '{teamId}' not found.");

        await _cache.RemoveAsync($"{TeamKeyPrefix}{teamId}");
        await _cache.RemoveByPrefixAsync(TeamsAllPrefix);

        _logger.LogInformation("Team {TeamId} updated ({AffectedRows} row(s) affected)", teamId, affectedRows);
    }

    public async Task DeleteAsync(Guid teamId)
    {
        _logger.LogDebug("Deleting team {TeamId}", teamId);

        var affectedRows = await _context.Teams
            .Where(t => t.TeamId == teamId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
            throw new KeyNotFoundException($"Team with id '{teamId}' not found.");

        await _cache.RemoveAsync($"{TeamKeyPrefix}{teamId}");
        await _cache.RemoveByPrefixAsync(TeamsAllPrefix);

        _logger.LogInformation("Team {TeamId} deleted ({AffectedRows} row(s) affected)", teamId, affectedRows);
    }

    private sealed class CachedTeamPage
    {
        public List<TeamDataModel> Items { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
