using cursor_dotnet_test.Persistence.DataModels;

namespace cursor_dotnet_test.Persistence.Repositories;

public interface ITeamRepository
{
    Task<TeamDataModel?> GetByIdAsync(Guid teamId);
    Task<(List<TeamDataModel> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string sortBy, bool ascending);
    Task<TeamDataModel> CreateAsync(TeamDataModel team);
    Task UpdateAsync(Guid teamId, string teamName, string managerName);
    Task DeleteAsync(Guid teamId);
}
