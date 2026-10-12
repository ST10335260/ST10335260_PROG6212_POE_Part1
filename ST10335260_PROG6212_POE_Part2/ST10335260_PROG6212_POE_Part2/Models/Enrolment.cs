namespace ST10335260_PROG6212_POE_Part2.Models;

public enum EnrolmentStatus { Pending, Confirmed, Cancelled }
public enum PaymentStatus { Pending, Paid, Refunded }

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public int ParticipantId { get; set; }
    public User? Participant { get; set; }
    public int EventCategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public int? BibNumber { get; set; }
}