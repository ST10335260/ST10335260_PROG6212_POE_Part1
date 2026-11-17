using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/organiser")]
public class OrganiserController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public OrganiserController(RaceDayDbContext context) => _context = context;

    [HttpGet("dashboard")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = CurrentUserId;

        var events = await EventSummaryBuilder.BuildAsync(
            _context,
            _context.Events.Where(e => e.OrganiserId == userId).OrderBy(e => e.EventDate));

        var now = DateTime.UtcNow;

        var pending = await _context.Enrolments
            .CountAsync(e => e.Event!.OrganiserId == userId && e.Status == EnrolmentStatus.Pending);

        var published = await _context.Results
            .CountAsync(r => r.Verified && r.Enrolment!.Event!.OrganiserId == userId);

        return Ok(new DashboardDto
        {
            TotalEvents = events.Count,
            TotalEnrolments = events.Sum(e => e.EnrolmentCount),
            UpcomingEvents = events.Count(e => e.EventDate >= now),
            PendingEnrolments = pending,
            ResultsPublished = published,
            Events = events
        });
    }
}
