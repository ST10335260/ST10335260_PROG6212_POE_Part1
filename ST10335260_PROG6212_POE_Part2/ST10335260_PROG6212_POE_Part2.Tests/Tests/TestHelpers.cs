using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

public class FakeBlobStorage : IBlobStorageService
{
    public List<string> Uploads { get; } = new();

    public Task<string> UploadAsync(string containerName, IFormFile file, CancellationToken cancellationToken = default)
    {
        var url = $"http://fake-storage/{containerName}/{file.FileName}";
        Uploads.Add(url);
        return Task.FromResult(url);
    }
}

public static class TestHelpers
{
    public const int OrganiserRoleId = 1;
    public const int ParticipantRoleId = 2;

    public static RaceDayDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new RaceDayDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static T WithSession<T>(T controller, int? userId, string? role) where T : ControllerBase
    {
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        if (userId.HasValue) httpContext.Session.SetInt32("UserId", userId.Value);
        if (role is not null) httpContext.Session.SetString("Role", role);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    public static User AddUser(RaceDayDbContext context, string email, string role = "Participant")
    {
        var user = new User
        {
            Email = email,
            PasswordHash = "hash",
            FirstName = "First",
            LastName = "Last",
            RoleId = role == "Organiser" ? OrganiserRoleId : ParticipantRoleId
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    public static Category AddCategory(RaceDayDbContext context, string name = "10km Road Run", decimal distance = 10)
    {
        var category = new Category { CategoryName = name, DefaultDistance = distance };
        context.Categories.Add(category);
        context.SaveChanges();
        return category;
    }

    public static Event AddEvent(RaceDayDbContext context, int organiserId, string name = "Test Event", int daysFromNow = 10, EventType type = EventType.Run)
    {
        var ev = new Event
        {
            OrganiserId = organiserId,
            EventName = name,
            Location = "Cape Town",
            DistanceKm = 10,
            EventDate = DateTime.UtcNow.AddDays(daysFromNow),
            EventType = type
        };
        context.Events.Add(ev);
        context.SaveChanges();
        return ev;
    }

    public static EventCategory AddEventCategory(RaceDayDbContext context, int eventId, int categoryId, int max = 100, int current = 0)
    {
        var ec = new EventCategory
        {
            EventId = eventId,
            CategoryId = categoryId,
            EntryFee = 100,
            MaxParticipants = max,
            CurrentParticipants = current
        };
        context.EventCategories.Add(ec);
        context.SaveChanges();
        return ec;
    }

    public static Enrolment AddEnrolment(RaceDayDbContext context, Event ev, EventCategory ec, User participant, EnrolmentStatus status = EnrolmentStatus.Confirmed, int bib = 1001)
    {
        var enrolment = new Enrolment
        {
            EventId = ev.EventId,
            EventCategoryId = ec.EventCategoryId,
            ParticipantId = participant.UserId,
            Status = status,
            BibNumber = bib
        };
        context.Enrolments.Add(enrolment);
        context.SaveChanges();
        return enrolment;
    }

    public static IFormFile NewImage(string fileName = "banner.png", string contentType = "image/png")
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
