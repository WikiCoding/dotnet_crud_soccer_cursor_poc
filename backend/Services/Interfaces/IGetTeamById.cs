using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface IGetTeamById
{
    Task<Team> GetTeamById(Guid teamId);
}
