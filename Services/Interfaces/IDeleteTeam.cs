namespace cursor_dotnet_test.Services.Interfaces;

public interface IDeleteTeam
{
    Task DeleteTeam(Guid teamId);
}
