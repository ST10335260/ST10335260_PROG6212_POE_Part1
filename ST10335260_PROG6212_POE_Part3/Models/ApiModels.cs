namespace ST10335260_PROG6212_POE_Part3.Models;

public enum EventType { Run, Walk, Cycle }

public enum EnrolmentStatus { Pending, Confirmed, Cancelled }

public enum PaymentStatus { Pending, Paid, Refunded }

public class LoginResponse
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UserProfile
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
}

public class EventSummary
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

    public bool IsPast => EventDate < DateTime.UtcNow;
}

public class CategoryInfo
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultDistance { get; set; }
}

public class EventCategoryInfo
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
    public int SpotsLeft { get; set; }

    public int PercentFull => MaxParticipants <= 0 ? 0 : Math.Min(100, (int)Math.Round(CurrentParticipants * 100.0 / MaxParticipants));
}

public class EnrolmentInfo
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

public class ResultInfo
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int? BibNumber { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int OverallRank { get; set; }
    public int? CategoryRank { get; set; }
    public decimal? PacePerKm { get; set; }
    public bool Verified { get; set; }
}

public class OrganiserEnrolmentInfo
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
    public ResultInfo? Result { get; set; }
}

public class MyResultInfo
{
    public int ResultId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int OverallRank { get; set; }
    public int TotalFinishers { get; set; }
    public decimal? PacePerKm { get; set; }
    public int? BibNumber { get; set; }
}

public class DashboardInfo
{
    public int TotalEvents { get; set; }
    public int TotalEnrolments { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingEnrolments { get; set; }
    public int ResultsPublished { get; set; }
    public List<EventSummary> Events { get; set; } = new();
}
