using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.DTOs;

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public int EventCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal EntryFee { get; set; }
    public EnrolmentStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int? BibNumber { get; set; }
    public DateTime EnrolmentDate { get; set; }
}

public class OrganiserEnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string ParticipantEmail { get; set; } = string.Empty;
    public int EventCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public EnrolmentStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public int? BibNumber { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public ResultDto? Result { get; set; }
}

public class UpdateEnrolmentStatusDto
{
    public EnrolmentStatus Status { get; set; }
}
