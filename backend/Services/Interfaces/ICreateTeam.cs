using cursor_dotnet_test.Domain;

namespace cursor_dotnet_test.Services.Interfaces;

public interface ICreateTeam
{
    Task<Team> CreateTeam(string teamName, string managerName);
}
