namespace ST10335260_PROG6212_POE_Part2.Models;

public class EventCategory
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; } = 100;
    public int CurrentParticipants { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}