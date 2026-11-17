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
public class EnrolmentsController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public EnrolmentsController(RaceDayDbContext context) => _context = context;

    private IQueryable<Enrolment> WithDetails() => _context.Enrolments
        .Include(e => e.Event)
        .Include(e => e.Participant)
        .Include(e => e.EventCategory).ThenInclude(ec => ec!.Category);

    [HttpPost("event-categories/{id:int}/enrolments")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Enrol(int id)
    {
        var ec = await _context.EventCategories.FindAsync(id);
        if (ec is null) return NotFound(new { message = "Event category not found." });

        var userId = CurrentUserId;

        bool alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e => e.ParticipantId == userId && e.EventCategoryId == id);
        if (alreadyEnrolled) return Conflict(new { message = "Already enrolled in this category." });

        if (ec.CurrentParticipants >= ec.MaxParticipants)
            return Conflict(new { message = "This category is full." });

        var ev = await _context.Events.FindAsync(ec.EventId);
        if (ev is null || !ev.IsActive) return NotFound(new { message = "Event not found." });

        if (ev.EventDate < DateTime.UtcNow)
            return Conflict(new { message = "This event has already taken place." });

        var highestBib = await _context.Enrolments
            .Where(e => e.EventId == ec.EventId)
            .MaxAsync(e => e.BibNumber);

        var enrolment = new Enrolment
        {
            EventId = ec.EventId,
            ParticipantId = userId,
            EventCategoryId = id,
            Status = EnrolmentStatus.Pending,
            BibNumber = (highestBib ?? 1000) + 1
        };

        ec.CurrentParticipants++;
        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        var saved = await WithDetails().FirstAsync(e => e.EnrolmentId == enrolment.EnrolmentId);
        return Created(string.Empty, EnrolmentMapper.ToDto(saved));
    }

    [HttpGet("users/me/enrolments")]
    [RequireAuth]
    public async Task<IActionResult> MyEnrolments()
    {
        var userId = CurrentUserId;
        var list = await WithDetails()
            .Where(e => e.ParticipantId == userId)
            .OrderBy(e => e.Event!.EventDate)
            .ToListAsync();
        return Ok(list.Select(EnrolmentMapper.ToDto));
    }

    [HttpGet("event-categories/{id:int}/enrolments")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> EnrolmentsForCategory(int id)
    {
        var ec = await _context.EventCategories.Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.EventCategoryId == id);
        if (ec is null) return NotFound(new { message = "Event category not found." });

        if (ec.Event is null || ec.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only view enrolments for your own events.");

        var list = await WithDetails()
            .Where(e => e.EventCategoryId == id)
            .OrderBy(e => e.BibNumber)
            .ToListAsync();

        var ids = list.Select(e => e.EnrolmentId).ToList();
        var results = await _context.Results.Where(r => ids.Contains(r.EnrolmentId)).ToListAsync();

        return Ok(list.Select(e => EnrolmentMapper.ToOrganiserDto(e, results.FirstOrDefault(r => r.EnrolmentId == e.EnrolmentId))));
    }

    [HttpPut("enrolments/{id:int}/status")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrolmentStatusDto dto)
    {
        if (!Enum.IsDefined(dto.Status))
            return BadRequest(new { message = "Status must be Pending, Confirmed or Cancelled." });

        var enrolment = await WithDetails().FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        if (enrolment.Event is null || enrolment.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only manage enrolments for your own events.");

        var previous = enrolment.Status;
        var category = enrolment.EventCategory;

        if (previous != dto.Status && category is not null)
        {
            if (dto.Status == EnrolmentStatus.Cancelled && category.CurrentParticipants > 0)
            {
                category.CurrentParticipants--;
            }
            else if (previous == EnrolmentStatus.Cancelled)
            {
                if (category.CurrentParticipants >= category.MaxParticipants)
                    return Conflict(new { message = "This category is full." });

                category.CurrentParticipants++;
            }
        }

        enrolment.Status = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(EnrolmentMapper.ToOrganiserDto(enrolment, null));
    }

    [HttpDelete("enrolments/{id:int}")]
    [RequireRole("Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

        if (enrolment.ParticipantId != CurrentUserId)
            return Forbidden("You can only cancel your own enrolments.");

        if (await _context.Results.AnyAsync(r => r.EnrolmentId == id))
            return Conflict(new { message = "A result has already been recorded for this enrolment." });

        var category = await _context.EventCategories.FindAsync(enrolment.EventCategoryId);
        if (category is not null && enrolment.Status != EnrolmentStatus.Cancelled && category.CurrentParticipants > 0)
            category.CurrentParticipants--;

        _context.Enrolments.Remove(enrolment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
