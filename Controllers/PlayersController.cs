using cursor_dotnet_test.DTOs;
using cursor_dotnet_test.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace cursor_dotnet_test.Controllers;

[ApiController]
[Route("players")]
[Authorize]
[EnableRateLimiting("players")]
public class PlayersController : ControllerBase
{
    private readonly ICreatePlayer _createPlayer;
    private readonly IGetPlayerById _getPlayerById;
    private readonly IGetPlayersByTeam _getPlayersByTeam;
    private readonly IUpdatePlayer _updatePlayer;
    private readonly IDeletePlayer _deletePlayer;

    public PlayersController(
        ICreatePlayer createPlayer,
        IGetPlayerById getPlayerById,
        IGetPlayersByTeam getPlayersByTeam,
        IUpdatePlayer updatePlayer,
        IDeletePlayer deletePlayer)
    {
        _createPlayer = createPlayer;
        _getPlayerById = getPlayerById;
        _getPlayersByTeam = getPlayersByTeam;
        _updatePlayer = updatePlayer;
        _deletePlayer = deletePlayer;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTeam([FromQuery] Guid teamId)
    {
        if (teamId == Guid.Empty)
            return BadRequest("teamId query parameter is required.");

        var players = await _getPlayersByTeam.GetPlayersByTeam(teamId);

        return Ok(players.Select(p => new PlayerResponse
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            PlayerPosition = p.PlayerPosition,
            PlayerAge = p.PlayerAge,
            TeamId = p.TeamId
        }).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var player = await _getPlayerById.GetPlayerById(id);

        return Ok(new PlayerResponse
        {
            PlayerId = player.PlayerId,
            PlayerName = player.PlayerName,
            PlayerPosition = player.PlayerPosition,
            PlayerAge = player.PlayerAge,
            TeamId = player.TeamId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request)
    {
        var player = await _createPlayer.CreatePlayer(
            request.PlayerName,
            request.PlayerPosition,
            request.PlayerAge,
            request.TeamId);

        var response = new PlayerResponse
        {
            PlayerId = player.PlayerId,
            PlayerName = player.PlayerName,
            PlayerPosition = player.PlayerPosition,
            PlayerAge = player.PlayerAge,
            TeamId = player.TeamId
        };

        return CreatedAtAction(nameof(GetById), new { id = response.PlayerId }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlayerRequest request)
    {
        await _updatePlayer.UpdatePlayer(
            id,
            request.PlayerName,
            request.PlayerPosition,
            request.PlayerAge,
            request.TeamId);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deletePlayer.DeletePlayer(id);
        return NoContent();
    }
}
