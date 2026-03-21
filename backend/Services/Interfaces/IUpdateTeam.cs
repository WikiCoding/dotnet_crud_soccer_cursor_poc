namespace cursor_dotnet_test.Services.Interfaces;

public interface IUpdateTeam
{
    Task UpdateTeam(Guid teamId, string teamName, string managerName);
}
