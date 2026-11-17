using ST10335260_PROG6212_POE_Part3.Models;

namespace ST10335260_PROG6212_POE_Part3.Services;

public record SessionUser(int UserId, string Role, string FullName, string Email, string? ProfilePictureUrl)
{
    public bool IsOrganiser => Role == "Organiser";

    public bool IsParticipant => Role == "Participant";

    public string FirstName => FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? FullName;

    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                0 => "?",
                1 => parts[0][..1].ToUpperInvariant(),
                _ => (parts[0][..1] + parts[^1][..1]).ToUpperInvariant()
            };
        }
    }
}

public static class SessionExtensions
{
    private const string UserIdKey = "UserId";
    private const string RoleKey = "Role";
    private const string NameKey = "FullName";
    private const string EmailKey = "Email";
    private const string PictureKey = "ProfilePictureUrl";

    public static void SignIn(this ISession session, LoginResponse user)
    {
        session.SetInt32(UserIdKey, user.UserId);
        session.SetString(RoleKey, user.Role);
        session.SetString(NameKey, $"{user.FirstName} {user.LastName}".Trim());
        session.SetString(EmailKey, user.Email);
        session.SetString(PictureKey, user.ProfilePictureUrl ?? string.Empty);
    }

    public static void UpdateProfile(this ISession session, string firstName, string lastName, string email, string? pictureUrl)
    {
        session.SetString(NameKey, $"{firstName} {lastName}".Trim());
        session.SetString(EmailKey, email);
        session.SetString(PictureKey, pictureUrl ?? string.Empty);
    }

    public static SessionUser? GetSessionUser(this ISession session)
    {
        var id = session.GetInt32(UserIdKey);
        var role = session.GetString(RoleKey);
        if (id is null || string.IsNullOrEmpty(role)) return null;

        var picture = session.GetString(PictureKey);
        return new SessionUser(
            id.Value,
            role,
            session.GetString(NameKey) ?? string.Empty,
            session.GetString(EmailKey) ?? string.Empty,
            string.IsNullOrEmpty(picture) ? null : picture);
    }
}
