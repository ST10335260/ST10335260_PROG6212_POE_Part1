using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public EventsController(RaceDayDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Events.Where(e => e.IsActive).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Create([FromBody] Event ev)
    {
        ev.OrganiserId = HttpContext.Session.GetInt32("UserId")!.Value;
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev);
    }

    [HttpPut("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] Event updated)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (ev.OrganiserId != userId) return Forbid();

        ev.EventName = updated.EventName;
        ev.Description = updated.Description;
        ev.EventDate = updated.EventDate;
        ev.Location = updated.Location;
        ev.DistanceKm = updated.DistanceKm;
        ev.EventType = updated.EventType;

        await _context.SaveChangesAsync();
        return Ok(ev);
    }

    [HttpDelete("{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev is null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        if (ev.OrganiserId != userId) return Forbid();

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}