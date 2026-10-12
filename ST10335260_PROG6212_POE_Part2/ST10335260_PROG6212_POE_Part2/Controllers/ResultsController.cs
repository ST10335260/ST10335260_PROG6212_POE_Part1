using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public ResultsController(RaceDayDbContext context) => _context = context;

    [HttpPost("results")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] Result dto)
    {
        var enrolment = await _context.Enrolments.FindAsync(dto.EnrolmentId);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        _context.Results.Add(dto);
        await _context.SaveChangesAsync();
        return Created(string.Empty, dto);
    }

    [HttpGet("event-categories/{id}/results")]
    public async Task<IActionResult> Leaderboard(int id)
    {
        var results = await _context.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment!.EventCategoryId == id)
            .OrderBy(r => r.Position)
            .ToListAsync();
        return Ok(results);
    }

    [HttpGet("users/me/results")]
    [RequireAuth]
    public async Task<IActionResult> MyResults()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var results = await _context.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment!.ParticipantId == userId)
            .ToListAsync();
        return Ok(results);
    }

    [HttpPut("results/{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] Result updated)
    {
        var result = await _context.Results.FindAsync(id);
        if (result is null) return NotFound();

        result.FinishTime = updated.FinishTime;
        result.Position = updated.Position;

        await _context.SaveChangesAsync();
        return Ok(result);
    }
}