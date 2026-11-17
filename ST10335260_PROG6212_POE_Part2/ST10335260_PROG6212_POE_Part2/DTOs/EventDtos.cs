using System.ComponentModel.DataAnnotations;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.DTOs;

public class EventRequestDto
{
    [Required, StringLength(150)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal? DistanceKm { get; set; }

    public EventType EventType { get; set; }

    public bool IsActive { get; set; } = true;
}

public class EventFilter
{
    public string? Search { get; set; }
    public EventType? Type { get; set; }
    public string? Location { get; set; }
    public decimal? MinDistance { get; set; }
    public decimal? MaxDistance { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool Upcoming { get; set; }
}

public class EventSummaryDto
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string? OrganiserName { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public string? BannerUrl { get; set; }
    public bool IsActive { get; set; }
    public int CategoryCount { get; set; }
    public decimal? MinEntryFee { get; set; }
    public int EnrolmentCount { get; set; }

    public static EventSummaryDto From(Event e, int categoryCount = 0, decimal? minEntryFee = null, int enrolmentCount = 0) => new()
    {
        EventId = e.EventId,
        OrganiserId = e.OrganiserId,
        OrganiserName = e.Organiser is null ? null : $"{e.Organiser.FirstName} {e.Organiser.LastName}".Trim(),
        EventName = e.EventName,
        Description = e.Description,
        EventDate = e.EventDate,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        BannerUrl = e.BannerUrl,
        IsActive = e.IsActive,
        CategoryCount = categoryCount,
        MinEntryFee = minEntryFee,
        EnrolmentCount = enrolmentCount
    };
}

public class DashboardDto
{
    public int TotalEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingEnrolments { get; set; }
    public int ResultsPublished { get; set; }
    public List<EventSummaryDto> Events { get; set; } = new();
}
