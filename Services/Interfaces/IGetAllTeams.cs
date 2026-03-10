using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface IGetAllTeams
{
    Task<(List<Team> Items, int TotalCount)> GetAllTeams(int page, int pageSize, string sortBy, bool ascending);
}
