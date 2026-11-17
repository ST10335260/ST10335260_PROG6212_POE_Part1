using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public EventsController(RaceDayDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventFilter filter)
    {
        var query = _context.Events.Where(e => e.IsActive);

        if (filter.Upcoming)
        {
            var now = DateTime.UtcNow;
            query = query.Where(e => e.EventDate >= now);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(e => e.EventName.Contains(term) || (e.Description != null && e.Description.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            var place = filter.Location.Trim();
            query = query.Where(e => e.Location.Contains(place));
        }

        if (filter.Type.HasValue)
            query = query.Where(e => e.EventType == filter.Type.Value);

        if (filter.MinDistance.HasValue)
            query = query.Where(e => e.DistanceKm >= filter.MinDistance.Value);

        if (filter.MaxDistance.HasValue)
            query = query.Where(e => e.DistanceKm <= filter.MaxDistance.Value);

        if (filter.From.HasValue)
            query = query.Where(e => e.EventDate >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.EventDate <= filter.To.Value);

        return Ok(await EventSummaryBuilder.BuildAsync(_context, query.OrderBy(e => e.EventDate)));
    }

    [HttpGet("mine")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> GetMine()
    {
        var userId = CurrentUserId;
        var query = _context.Events.Where(e => e.OrganiserId == userId).OrderBy(e => e.EventDate);
        return Ok(await EventSummaryBuilder.BuildAsync(_context, query));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var list = await EventSummaryBuilder.BuildAsync(_context, _context.Events.Where(e => e.EventId == id));
        var ev = list.FirstOrDefault();

        if (ev is null) return NotFound(new { message = "Event not found." });
        if (!ev.IsActive && ev.OrganiserId != CurrentUserId) return NotFound(new { message = "Event not found." });

        return Ok(ev);
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] EventRequestDto dto)
    {
        var ev = new Event
        {
            OrganiserId = CurrentUserId,
            EventName = dto.EventName.Trim(),
            Description = dto.Description?.Trim(),
            EventDate = dto.EventDate,
            Location = dto.Location.Trim(),
            DistanceKm = dto.DistanceKm,
            EventType = dto.EventType,
            IsActive = dto.IsActive
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, EventSummaryDto.From(ev));
    }

    [HttpPut("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] EventRequestDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only change your own events.");

        ev.EventName = dto.EventName.Trim();
        ev.Description = dto.Description?.Trim();
        ev.EventDate = dto.EventDate;
        ev.Location = dto.Location.Trim();
        ev.DistanceKm = dto.DistanceKm;
        ev.EventType = dto.EventType;
        ev.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Ok(EventSummaryDto.From(ev));
    }

    [HttpDelete("{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only delete your own events.");

        if (await _context.Enrolments.AnyAsync(e => e.EventId == id))
            return Conflict(new { message = "This event already has enrolments and cannot be deleted." });

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/banner")]
    [RequireRole("Organiser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(int id, [FromForm] ImageUploadDto dto, [FromServices] IBlobStorageService storage)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only change your own events.");

        var problem = ImageValidator.Validate(dto.File);
        if (problem is not null) return BadRequest(new { message = problem });

        ev.BannerUrl = await storage.UploadAsync(BlobContainers.EventBanners, dto.File);
        await _context.SaveChangesAsync();

        return Ok(new UploadResultDto { Url = ev.BannerUrl });
    }

    [HttpGet("{id:int}/enrolments")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> GetEnrolments(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only view enrolments for your own events.");

        var enrolments = await _context.Enrolments
            .Include(e => e.Participant)
            .Include(e => e.EventCategory).ThenInclude(ec => ec!.Category)
            .Where(e => e.EventId == id)
            .OrderBy(e => e.EventCategoryId).ThenBy(e => e.BibNumber)
            .ToListAsync();

        var ids = enrolments.Select(e => e.EnrolmentId).ToList();
        var results = await _context.Results.Where(r => ids.Contains(r.EnrolmentId)).ToListAsync();

        return Ok(enrolments.Select(e => EnrolmentMapper.ToOrganiserDto(e, results.FirstOrDefault(r => r.EnrolmentId == e.EnrolmentId))));
    }
}
