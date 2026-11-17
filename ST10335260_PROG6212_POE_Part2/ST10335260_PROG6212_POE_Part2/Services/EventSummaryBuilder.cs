using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Services;

public static class EventSummaryBuilder
{
    public static async Task<List<EventSummaryDto>> BuildAsync(RaceDayDbContext context, IQueryable<Event> query)
    {
        var events = await query.Include(e => e.Organiser).ToListAsync();
        if (events.Count == 0) return new List<EventSummaryDto>();

        var ids = events.Select(e => e.EventId).ToList();

        var categoryStats = await context.EventCategories
            .Where(ec => ids.Contains(ec.EventId))
            .GroupBy(ec => ec.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count(), MinFee = g.Min(x => x.EntryFee) })
            .ToListAsync();

        var enrolmentStats = await context.Enrolments
            .Where(en => ids.Contains(en.EventId) && en.Status != EnrolmentStatus.Cancelled)
            .GroupBy(en => en.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToListAsync();

        return events.Select(e =>
        {
            var categories = categoryStats.FirstOrDefault(x => x.EventId == e.EventId);
            var enrolments = enrolmentStats.FirstOrDefault(x => x.EventId == e.EventId);
            return EventSummaryDto.From(
                e,
                categories?.Count ?? 0,
                categories is null ? null : categories.MinFee,
                enrolments?.Count ?? 0);
        }).ToList();
    }
}
