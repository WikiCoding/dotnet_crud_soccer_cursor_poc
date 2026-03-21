using cursor_dotnet_test.DTOs;
using cursor_dotnet_test.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cursor_dotnet_test.Controllers;

[ApiController]
[Route("teams")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ICreateTeam _createTeam;
    private readonly IGetTeamById _getTeamById;
    private readonly IGetAllTeams _getAllTeams;
    private readonly IUpdateTeam _updateTeam;
    private readonly IDeleteTeam _deleteTeam;

    public TeamsController(
        ICreateTeam createTeam,
        IGetTeamById getTeamById,
        IGetAllTeams getAllTeams,
        IUpdateTeam updateTeam,
        IDeleteTeam deleteTeam)
    {
        _createTeam = createTeam;
        _getTeamById = getTeamById;
        _getAllTeams = getAllTeams;
        _updateTeam = updateTeam;
        _deleteTeam = deleteTeam;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "teamName",
        [FromQuery] string sortDirection = "asc")
    {
        if (page < 1) return BadRequest("Page must be >= 1.");
        if (pageSize < 1 || pageSize > 100) return BadRequest("PageSize must be between 1 and 100.");

        var ascending = !sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var (items, totalCount) = await _getAllTeams.GetAllTeams(page, pageSize, sortBy, ascending);

        return Ok(new PaginatedResponse<TeamResponse>
        {
            Items = items.Select(t => new TeamResponse
            {
                TeamId = t.TeamId,
                TeamName = t.TeamName,
                ManagerName = t.ManagerName
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var team = await _getTeamById.GetTeamById(id);

        return Ok(new TeamResponse
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            ManagerName = team.ManagerName
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
    {
        var team = await _createTeam.CreateTeam(request.TeamName, request.ManagerName);

        var response = new TeamResponse
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            ManagerName = team.ManagerName
        };

        return CreatedAtAction(nameof(GetById), new { id = response.TeamId }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request)
    {
        await _updateTeam.UpdateTeam(id, request.TeamName, request.ManagerName);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _deleteTeam.DeleteTeam(id);
        return NoContent();
    }
}
