using cursor_dotnet_test.Domain;
using cursor_dotnet_test.Persistence.DataModels;
using cursor_dotnet_test.Persistence.Repositories;
using cursor_dotnet_test.Services.Interfaces;

namespace cursor_dotnet_test.Services;

public class TeamsService : ICreateTeam, IGetTeamById, IGetAllTeams, IUpdateTeam, IDeleteTeam
{
    private readonly ITeamRepository _repository;
    private readonly ILogger<TeamsService> _logger;

    public TeamsService(ITeamRepository repository, ILogger<TeamsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(List<Team> Items, int TotalCount)> GetAllTeams(int page, int pageSize, string sortBy, bool ascending)
    {
        _logger.LogInformation("Retrieving teams page {Page}, size {PageSize}, sort {SortBy} {Direction}",
            page, pageSize, sortBy, ascending ? "ASC" : "DESC");

        var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, sortBy, ascending);

        return (items.Select(ToTeam).ToList(), totalCount);
    }

    public async Task<Team> CreateTeam(string teamName, string managerName)
    {
        _logger.LogInformation("Creating team with name '{TeamName}' and manager '{ManagerName}'", teamName, managerName);

        var dataModel = new TeamDataModel
        {
            TeamId = Guid.NewGuid(),
            TeamName = teamName,
            ManagerName = managerName,
            Version = 1
        };

        var created = await _repository.CreateAsync(dataModel);

        _logger.LogInformation("Team '{TeamId}' created successfully", created.TeamId);
        return ToTeam(created);
    }

    public async Task<Team> GetTeamById(Guid teamId)
    {
        _logger.LogInformation("Retrieving team '{TeamId}'", teamId);

        var dataModel = await _repository.GetByIdAsync(teamId)
            ?? throw new KeyNotFoundException($"Team with id '{teamId}' not found.");

        return ToTeam(dataModel);
    }

    public async Task UpdateTeam(Guid teamId, string teamName, string managerName)
    {
        _logger.LogInformation("Updating team '{TeamId}'", teamId);
        await _repository.UpdateAsync(teamId, teamName, managerName);
        _logger.LogInformation("Team '{TeamId}' updated successfully", teamId);
    }

    public async Task DeleteTeam(Guid teamId)
    {
        _logger.LogInformation("Deleting team '{TeamId}'", teamId);
        await _repository.DeleteAsync(teamId);
        _logger.LogInformation("Team '{TeamId}' deleted successfully", teamId);
    }

    private static Team ToTeam(TeamDataModel model) => new()
    {
        TeamId = model.TeamId,
        TeamName = model.TeamName,
        ManagerName = model.ManagerName
    };
}
