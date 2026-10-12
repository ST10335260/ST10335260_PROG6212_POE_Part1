using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public EnrolmentsController(RaceDayDbContext context) => _context = context;

    [HttpPost("event-categories/{id}/enrolments")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Enrol(int id)
    {
        var ec = await _context.EventCategories.FindAsync(id);
        if (ec is null) return NotFound(new { message = "Event category not found." });

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;

        bool alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e => e.ParticipantId == userId && e.EventCategoryId == id);
        if (alreadyEnrolled) return Conflict(new { message = "Already enrolled in this category." });

        if (ec.CurrentParticipants >= ec.MaxParticipants)
            return Conflict(new { message = "This category is full." });

        var enrolment = new Enrolment
        {
            EventId = ec.EventId,
            ParticipantId = userId,
            EventCategoryId = id,
            Status = EnrolmentStatus.Confirmed,
            BibNumber = 1000 + await _context.Enrolments.CountAsync() + 1
        };

        ec.CurrentParticipants++;
        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return Created(string.Empty, enrolment);
    }

    [HttpGet("users/me/enrolments")]
    [RequireAuth]
    public async Task<IActionResult> MyEnrolments()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var list = await _context.Enrolments
            .Where(e => e.ParticipantId == userId)
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("event-categories/{id}/enrolments")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> EnrolmentsForCategory(int id)
    {
        var list = await _context.Enrolments
            .Where(e => e.EventCategoryId == id)
            .ToListAsync();
        return Ok(list);
    }

    [HttpDelete("enrolments/{id}")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment is null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (enrolment.ParticipantId != userId) return Forbid();

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}