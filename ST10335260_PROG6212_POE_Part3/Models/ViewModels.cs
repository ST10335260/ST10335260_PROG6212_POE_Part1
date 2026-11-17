using System.ComponentModel.DataAnnotations;
using ST10335260_PROG6212_POE_Part3.Helpers;

namespace ST10335260_PROG6212_POE_Part3.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Enter your first name."), StringLength(60)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your last name."), StringLength(60)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your email address."), StringLength(150)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your date of birth.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Choose how you will use RaceDay.")]
    public string Role { get; set; } = "Participant";

    [Required(ErrorMessage = "Choose a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Your password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Your password must include a letter and a number.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Enter your first name."), StringLength(60)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your last name."), StringLength(60)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your email address."), StringLength(150)]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}

public class EventFilterViewModel
{
    public string? Search { get; set; }
    public string? Location { get; set; }
    public EventType? Type { get; set; }
    public string? Distance { get; set; }
    public DateTime? From { get; set; }

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrWhiteSpace(Location) ||
        Type.HasValue || !string.IsNullOrWhiteSpace(Distance) || From.HasValue;

    public string ToApiPath()
    {
        var parts = new List<string> { "upcoming=true" };

        if (!string.IsNullOrWhiteSpace(Search)) parts.Add("search=" + Uri.EscapeDataString(Search.Trim()));
        if (!string.IsNullOrWhiteSpace(Location)) parts.Add("location=" + Uri.EscapeDataString(Location.Trim()));
        if (Type.HasValue) parts.Add("type=" + Type.Value);
        if (From.HasValue) parts.Add("from=" + From.Value.ToString("yyyy-MM-dd"));

        switch (Distance)
        {
            case "upto5": parts.Add("maxDistance=5"); break;
            case "5to10": parts.Add("minDistance=5"); parts.Add("maxDistance=10"); break;
            case "10to21": parts.Add("minDistance=10"); parts.Add("maxDistance=21.1"); break;
            case "21plus": parts.Add("minDistance=21.1"); break;
        }

        return "api/events?" + string.Join("&", parts);
    }
}

public class HomeViewModel
{
    public EventFilterViewModel Filter { get; set; } = new();
    public List<EventSummary> Events { get; set; } = new();
    public bool LoadFailed { get; set; }
}

public class EventDetailsViewModel
{
    public EventSummary Event { get; set; } = new();
    public List<EventCategoryInfo> Categories { get; set; } = new();
    public Dictionary<int, EnrolmentInfo> MyEnrolments { get; set; } = new();
    public Dictionary<int, List<ResultInfo>> Leaderboards { get; set; } = new();
}

public class EventFormViewModel
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Give the event a name."), StringLength(150)]
    [Display(Name = "Event name")]
    public string EventName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Choose the date and start time.")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Date and start time")]
    public DateTime? EventDate { get; set; }

    [Required(ErrorMessage = "Enter the location."), StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the distance in kilometres.")]
    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKm { get; set; }

    [Display(Name = "Event type")]
    public EventType EventType { get; set; }

    [Display(Name = "Visible to participants")]
    public bool IsActive { get; set; } = true;

    public string? BannerUrl { get; set; }

    public IFormFile? Banner { get; set; }

    public bool IsEdit => EventId > 0;
}

public class AddEventCategoryForm
{
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
    public int CategoryId { get; set; }

    [Range(0, 100000, ErrorMessage = "Entry fee must be between R0 and R100 000.")]
    public decimal EntryFee { get; set; }

    [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
    public int MaxParticipants { get; set; } = 100;
}

public class EditEventCategoryForm
{
    public int EventId { get; set; }
    public int EventCategoryId { get; set; }
    public int CategoryId { get; set; }

    [Range(0, 100000, ErrorMessage = "Entry fee must be between R0 and R100 000.")]
    public decimal EntryFee { get; set; }

    [Range(1, 100000, ErrorMessage = "Maximum participants must be at least 1.")]
    public int MaxParticipants { get; set; }
}

public class NewCategoryForm
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Give the category a name."), StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
    public decimal? DefaultDistance { get; set; }
}

public class CategoriesPageViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public EventSummary? Selected { get; set; }
    public List<EventCategoryInfo> EventCategories { get; set; } = new();
    public List<CategoryInfo> AvailableCategories { get; set; } = new();
}

public class EnrolmentsPageViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public EventSummary? Selected { get; set; }
    public List<OrganiserEnrolmentInfo> Enrolments { get; set; } = new();
    public EnrolmentStatus? StatusFilter { get; set; }
}

public class ResultsPageViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public EventSummary? Selected { get; set; }
    public List<OrganiserEnrolmentInfo> Enrolments { get; set; } = new();
}

public class ResultFormViewModel
{
    public int EventId { get; set; }
    public int EnrolmentId { get; set; }
    public int ResultId { get; set; }

    [Required(ErrorMessage = "Enter the finish time as hh:mm:ss.")]
    [RegularExpression(@"^\d{1,2}:[0-5]\d:[0-5]\d$", ErrorMessage = "Finish time must look like 00:52:30.")]
    public string FinishTime { get; set; } = string.Empty;

    [Range(1, 100000, ErrorMessage = "Position must be 1 or higher.")]
    public int Position { get; set; }

    public bool Verified { get; set; }
}

public class EventPickerViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public int? SelectedId { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
}
