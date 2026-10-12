using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public CategoriesController(RaceDayDbContext context) => _context = context;

    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories() =>
        Ok(await _context.Categories.ToListAsync());

    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var list = await _context.EventCategories
            .Include(ec => ec.Category)
            .Where(ec => ec.EventId == eventId)
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("events/{eventId}/categories")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> AddToEvent(int eventId, [FromBody] EventCategory dto)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        dto.EventId = eventId;
        _context.EventCategories.Add(dto);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetByEvent), new { eventId }, dto);
    }

    [HttpPut("event-categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] EventCategory updated)
    {
        var ec = await _context.EventCategories.FindAsync(id);
        if (ec is null) return NotFound();

        ec.EntryFee = updated.EntryFee;
        ec.MaxParticipants = updated.MaxParticipants;

        await _context.SaveChangesAsync();
        return Ok(ec);
    }

    [HttpDelete("event-categories/{id}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ec = await _context.EventCategories.FindAsync(id);
        if (ec is null) return NotFound();

        _context.EventCategories.Remove(ec);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}