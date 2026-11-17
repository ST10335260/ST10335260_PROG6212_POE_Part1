using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Seeding;

public static class DbSeeder
{
    public const string DemoPassword = "Password123!";

    public static async Task SeedAsync(RaceDayDbContext db, bool includeDemoData)
    {
        await SeedCategoriesAsync(db);

        if (includeDemoData && !await db.Users.AnyAsync())
            await SeedDemoDataAsync(db);
    }

    private static async Task SeedCategoriesAsync(RaceDayDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        db.Categories.AddRange(
            new Category { CategoryName = "5km Fun Run", Description = "Beginner friendly 5km", DefaultDistance = 5 },
            new Category { CategoryName = "10km Road Run", Description = "Standard 10km road race", DefaultDistance = 10 },
            new Category { CategoryName = "21.1km Half Marathon", Description = "Half marathon distance", DefaultDistance = 21.1m });

        await db.SaveChangesAsync();
    }

    private static async Task SeedDemoDataAsync(RaceDayDbContext db)
    {
        var organiserRole = await db.Roles.FirstAsync(r => r.RoleName == "Organiser");
        var participantRole = await db.Roles.FirstAsync(r => r.RoleName == "Participant");
        var hash = PasswordHasher.Hash(DemoPassword);

        var organiser = new User { Email = "organiser@raceday.demo", PasswordHash = hash, FirstName = "Lerato", LastName = "Mokoena", DateOfBirth = new DateTime(1988, 4, 12), RoleId = organiserRole.RoleId };
        var thandi = new User { Email = "thandi@raceday.demo", PasswordHash = hash, FirstName = "Thandi", LastName = "Nkosi", DateOfBirth = new DateTime(1995, 9, 3), RoleId = participantRole.RoleId };
        var sipho = new User { Email = "sipho@raceday.demo", PasswordHash = hash, FirstName = "Sipho", LastName = "Dlamini", DateOfBirth = new DateTime(1992, 1, 21), RoleId = participantRole.RoleId };

        db.Users.AddRange(organiser, thandi, sipho);
        await db.SaveChangesAsync();

        var fiveK = await db.Categories.FirstAsync(c => c.CategoryName == "5km Fun Run");
        var tenK = await db.Categories.FirstAsync(c => c.CategoryName == "10km Road Run");
        var half = await db.Categories.FirstAsync(c => c.CategoryName == "21.1km Half Marathon");

        var today = DateTime.UtcNow.Date;

        var sunrise = new Event { OrganiserId = organiser.UserId, EventName = "Table Mountain Sunrise Run", Description = "A scenic early morning run along the lower slopes of Table Mountain with views over the city bowl.", EventDate = today.AddDays(21).AddHours(6), Location = "Cape Town", DistanceKm = 10, EventType = EventType.Run };
        var nightWalk = new Event { OrganiserId = organiser.UserId, EventName = "Jozi Night Walk", Description = "A relaxed evening walk through the lit-up streets of Johannesburg. Families and groups welcome.", EventDate = today.AddDays(35).AddHours(18).AddMinutes(30), Location = "Johannesburg", DistanceKm = 5, EventType = EventType.Walk };
        var cycle = new Event { OrganiserId = organiser.UserId, EventName = "Garden Route Gran Fondo", Description = "A rolling coastal cycle through forest and farmland on closed roads.", EventDate = today.AddDays(60).AddHours(7), Location = "George", DistanceKm = 60, EventType = EventType.Cycle };
        var past = new Event { OrganiserId = organiser.UserId, EventName = "Durban Beachfront 10K", Description = "A fast, flat 10km along the Durban beachfront promenade.", EventDate = today.AddDays(-30).AddHours(7), Location = "Durban", DistanceKm = 10, EventType = EventType.Run };

        db.Events.AddRange(sunrise, nightWalk, cycle, past);
        await db.SaveChangesAsync();

        var sunrise5 = new EventCategory { EventId = sunrise.EventId, CategoryId = fiveK.CategoryId, EntryFee = 120, MaxParticipants = 150 };
        var sunrise10 = new EventCategory { EventId = sunrise.EventId, CategoryId = tenK.CategoryId, EntryFee = 180, MaxParticipants = 200 };
        var night5 = new EventCategory { EventId = nightWalk.EventId, CategoryId = fiveK.CategoryId, EntryFee = 90, MaxParticipants = 300 };
        var cycleHalf = new EventCategory { EventId = cycle.EventId, CategoryId = half.CategoryId, EntryFee = 450, MaxParticipants = 120 };
        var past10 = new EventCategory { EventId = past.EventId, CategoryId = tenK.CategoryId, EntryFee = 150, MaxParticipants = 100 };

        db.EventCategories.AddRange(sunrise5, sunrise10, night5, cycleHalf, past10);
        await db.SaveChangesAsync();

        var e1 = new Enrolment { EventId = sunrise.EventId, ParticipantId = thandi.UserId, EventCategoryId = sunrise10.EventCategoryId, Status = EnrolmentStatus.Confirmed, PaymentStatus = PaymentStatus.Paid, BibNumber = 1001 };
        var e2 = new Enrolment { EventId = sunrise.EventId, ParticipantId = sipho.UserId, EventCategoryId = sunrise5.EventCategoryId, Status = EnrolmentStatus.Pending, BibNumber = 1002 };
        var e3 = new Enrolment { EventId = past.EventId, ParticipantId = thandi.UserId, EventCategoryId = past10.EventCategoryId, Status = EnrolmentStatus.Confirmed, PaymentStatus = PaymentStatus.Paid, BibNumber = 1001, EnrolmentDate = today.AddDays(-50) };
        var e4 = new Enrolment { EventId = past.EventId, ParticipantId = sipho.UserId, EventCategoryId = past10.EventCategoryId, Status = EnrolmentStatus.Confirmed, PaymentStatus = PaymentStatus.Paid, BibNumber = 1002, EnrolmentDate = today.AddDays(-48) };

        sunrise10.CurrentParticipants = 1;
        sunrise5.CurrentParticipants = 1;
        past10.CurrentParticipants = 2;

        db.Enrolments.AddRange(e1, e2, e3, e4);
        await db.SaveChangesAsync();

        db.Results.AddRange(
            new Result { EnrolmentId = e3.EnrolmentId, FinishTime = new TimeSpan(0, 49, 12), Position = 1, OverallRank = 1, CategoryRank = 1, PacePerKm = 4.92m, Verified = true },
            new Result { EnrolmentId = e4.EnrolmentId, FinishTime = new TimeSpan(0, 55, 40), Position = 2, OverallRank = 2, CategoryRank = 2, PacePerKm = 5.57m, Verified = true });

        await db.SaveChangesAsync();
    }
}
