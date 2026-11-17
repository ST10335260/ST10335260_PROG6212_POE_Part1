using System.ComponentModel.DataAnnotations;

namespace ST10335260_PROG6212_POE_Part2.DTOs;

public class CategoryRequestDto
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.1, 1000)]
    public decimal? DefaultDistance { get; set; }
}

public class EventCategoryRequestDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    [Range(1, 100000)]
    public int MaxParticipants { get; set; } = 100;
}

public class EventCategoryDto
{
    public int EventCategoryId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultDistance { get; set; }
    public decimal EntryFee { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public int SpotsLeft => Math.Max(0, MaxParticipants - CurrentParticipants);
}
