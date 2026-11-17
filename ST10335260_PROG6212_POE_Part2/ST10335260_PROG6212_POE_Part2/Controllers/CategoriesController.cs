using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public CategoriesController(RaceDayDbContext context) => _context = context;

    private static EventCategoryDto ToDto(EventCategory ec) => new()
    {
        EventCategoryId = ec.EventCategoryId,
        EventId = ec.EventId,
        CategoryId = ec.CategoryId,
        CategoryName = ec.Category?.CategoryName ?? string.Empty,
        Description = ec.Category?.Description,
        DefaultDistance = ec.Category?.DefaultDistance,
        EntryFee = ec.EntryFee,
        MaxParticipants = ec.MaxParticipants,
        CurrentParticipants = ec.CurrentParticipants
    };

    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategories() =>
        Ok(await _context.Categories.OrderBy(c => c.DefaultDistance).ThenBy(c => c.CategoryName).ToListAsync());

    [HttpPost("categories")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDto dto)
    {
        var name = dto.CategoryName.Trim();
        if (await _context.Categories.AnyAsync(c => c.CategoryName == name))
            return Conflict(new { message = "A category with that name already exists." });

        var category = new Category
        {
            CategoryName = name,
            Description = dto.Description?.Trim(),
            DefaultDistance = dto.DefaultDistance
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Created(string.Empty, category);
    }

    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var list = await _context.EventCategories
            .Include(ec => ec.Category)
            .Where(ec => ec.EventId == eventId)
            .OrderBy(ec => ec.Category!.DefaultDistance)
            .ToListAsync();
        return Ok(list.Select(ToDto));
    }

    [HttpPost("events/{eventId:int}/categories")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> AddToEvent(int eventId, [FromBody] EventCategoryRequestDto dto)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.OrganiserId != CurrentUserId)
            return Forbidden("You can only add categories to your own events.");

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        if (category is null) return NotFound(new { message = "Category not found." });

        if (await _context.EventCategories.AnyAsync(ec => ec.EventId == eventId && ec.CategoryId == dto.CategoryId))
            return Conflict(new { message = "This category has already been added to the event." });

        var eventCategory = new EventCategory
        {
            EventId = eventId,
            CategoryId = dto.CategoryId,
            EntryFee = dto.EntryFee,
            MaxParticipants = dto.MaxParticipants
        };

        _context.EventCategories.Add(eventCategory);
        await _context.SaveChangesAsync();

        eventCategory.Category = category;
        return CreatedAtAction(nameof(GetByEvent), new { eventId }, ToDto(eventCategory));
    }

    [HttpPut("event-categories/{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] EventCategoryRequestDto dto)
    {
        var ec = await _context.EventCategories.Include(x => x.Category).Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.EventCategoryId == id);
        if (ec is null) return NotFound(new { message = "Event category not found." });

        if (ec.Event is null || ec.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only change categories on your own events.");

        if (dto.MaxParticipants < ec.CurrentParticipants)
            return Conflict(new { message = "Maximum participants cannot be lower than the number already enrolled." });

        ec.EntryFee = dto.EntryFee;
        ec.MaxParticipants = dto.MaxParticipants;

        await _context.SaveChangesAsync();
        return Ok(ToDto(ec));
    }

    [HttpDelete("event-categories/{id:int}")]
    [RequireRole("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var ec = await _context.EventCategories.Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.EventCategoryId == id);
        if (ec is null) return NotFound(new { message = "Event category not found." });

        if (ec.Event is null || ec.Event.OrganiserId != CurrentUserId)
            return Forbidden("You can only remove categories from your own events.");

        if (await _context.Enrolments.AnyAsync(e => e.EventCategoryId == id))
            return Conflict(new { message = "This category already has enrolments and cannot be removed." });

        _context.EventCategories.Remove(ec);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
