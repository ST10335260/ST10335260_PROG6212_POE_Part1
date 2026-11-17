using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public ResultsController(RaceDayDbContext context) => _context = context;

    private static decimal? CalculatePace(TimeSpan finishTime, Enrolment enrolment)
    {
        var distance = enrolment.EventCategory?.Category?.DefaultDistance ?? enrolment.Event?.DistanceKm;
        if (distance is null || distance <= 0) return null;
        return Math.Round((decimal)finishTime.TotalMinutes / distance.Value, 2);
    }

    private Task<Enrolment?> LoadEnrolment(int enrolmentId) => _context.Enrolments
        .Include(e => e.Event)
        .Include(e => e.Participant)
        .Include(e => e.EventCategory).ThenInclude(ec => ec!.Category)
        .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

    [HttpPost("results")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] ResultRequestDto dto)
    {
        var enrolment = await LoadEnrolment(dto.EnrolmentId);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        if (enrolment.Event is null || enrolment.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only capture results for your own events.");

        if (enrolment.Status != EnrolmentStatus.Confirmed)
            return Conflict(new { message = "Only confirmed enrolments can receive a result." });

        if (await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId))
            return Conflict(new { message = "A result has already been recorded for this enrolment." });

        if (dto.FinishTime <= TimeSpan.Zero)
            return BadRequest(new { message = "Finish time must be greater than zero." });

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            OverallRank = dto.OverallRank ?? dto.Position,
            CategoryRank = dto.CategoryRank ?? dto.Position,
            PacePerKm = dto.PacePerKm ?? CalculatePace(dto.FinishTime, enrolment),
            Verified = dto.Verified
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return Created(string.Empty, ResultMapper.ToDto(result, enrolment));
    }

    [HttpGet("event-categories/{id:int}/results")]
    public async Task<IActionResult> Leaderboard(int id)
    {
        var showAll = CurrentRole == "Organiser";

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
            .Include(r => r.Enrolment).ThenInclude(e => e!.EventCategory).ThenInclude(ec => ec!.Category)
            .Where(r => r.Enrolment!.EventCategoryId == id && (showAll || r.Verified))
            .OrderBy(r => r.Position)
            .ToListAsync();

        return Ok(results.Select(r => ResultMapper.ToDto(r)));
    }

    [HttpGet("users/me/results")]
    [RequireAuth]
    public async Task<IActionResult> MyResults()
    {
        var userId = CurrentUserId;

        var results = await _context.Results
            .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e!.EventCategory).ThenInclude(ec => ec!.Category)
            .Where(r => r.Enrolment!.ParticipantId == userId && r.Verified)
            .ToListAsync();

        var categoryIds = results.Select(r => r.Enrolment!.EventCategoryId).Distinct().ToList();
        var totals = await _context.Results
            .Where(r => r.Verified && categoryIds.Contains(r.Enrolment!.EventCategoryId))
            .GroupBy(r => r.Enrolment!.EventCategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();

        var list = results
            .OrderByDescending(r => r.Enrolment!.Event?.EventDate)
            .Select(r => new MyResultDto
            {
                ResultId = r.ResultId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.Event?.EventName ?? string.Empty,
                EventDate = r.Enrolment.Event?.EventDate ?? default,
                CategoryName = r.Enrolment.EventCategory?.Category?.CategoryName ?? string.Empty,
                FinishTime = r.FinishTime,
                Position = r.Position,
                OverallRank = r.OverallRank,
                TotalFinishers = totals.FirstOrDefault(t => t.CategoryId == r.Enrolment.EventCategoryId)?.Count ?? 0,
                PacePerKm = r.PacePerKm,
                BibNumber = r.Enrolment.BibNumber
            })
            .ToList();

        return Ok(list);
    }

    [HttpPut("results/{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] ResultRequestDto dto)
    {
        var result = await _context.Results.FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null) return NotFound(new { message = "Result not found." });

        var enrolment = await LoadEnrolment(result.EnrolmentId);
        if (enrolment?.Event is null || enrolment.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only change results for your own events.");

        if (dto.FinishTime <= TimeSpan.Zero)
            return BadRequest(new { message = "Finish time must be greater than zero." });

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.OverallRank = dto.OverallRank ?? dto.Position;
        result.CategoryRank = dto.CategoryRank ?? dto.Position;
        result.PacePerKm = dto.PacePerKm ?? CalculatePace(dto.FinishTime, enrolment);
        result.Verified = dto.Verified;

        await _context.SaveChangesAsync();
        return Ok(ResultMapper.ToDto(result, enrolment));
    }
}
