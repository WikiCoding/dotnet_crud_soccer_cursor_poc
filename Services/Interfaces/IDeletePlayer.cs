namespace cursor_dotnet_test.Services.Interfaces;

public interface IDeletePlayer
{
    Task DeletePlayer(Guid playerId);
}
