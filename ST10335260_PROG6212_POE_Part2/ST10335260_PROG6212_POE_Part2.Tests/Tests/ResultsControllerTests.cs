using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;
using Xunit;

public class ResultsControllerTests
{
    private static ResultRequestDto Request(int enrolmentId, bool verified = true) => new()
    {
        EnrolmentId = enrolmentId,
        FinishTime = new TimeSpan(0, 50, 0),
        Position = 3,
        Verified = verified
    };

    [Fact]
    public async Task Create_ForConfirmedEnrolment_CalculatesPaceAndDefaultsRanks()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context, "10km Road Run", 10);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new ResultsController(context), organiser.UserId, "Organiser");

        var result = await controller.Create(Request(enrolment.EnrolmentId));

        var created = Assert.IsType<CreatedResult>(result);
        var dto = Assert.IsType<ResultDto>(created.Value);
        Assert.Equal(5.00m, dto.PacePerKm);
        Assert.Equal(3, dto.OverallRank);
        Assert.Equal(3, dto.Position);
    }

    [Fact]
    public async Task Create_ForPendingEnrolment_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant, EnrolmentStatus.Pending);
        var controller = TestHelpers.WithSession(new ResultsController(context), organiser.UserId, "Organiser");

        var result = await controller.Create(Request(enrolment.EnrolmentId));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_WhenResultAlreadyExists_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        context.Results.Add(new Result { EnrolmentId = enrolment.EnrolmentId, FinishTime = new TimeSpan(0, 45, 0), Position = 1, OverallRank = 1 });
        context.SaveChanges();
        var controller = TestHelpers.WithSession(new ResultsController(context), organiser.UserId, "Organiser");

        var result = await controller.Create(Request(enrolment.EnrolmentId));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_ByDifferentOrganiser_Returns403()
    {
        var context = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(context, "owner@test.com", "Organiser");
        var other = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, owner.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new ResultsController(context), other.UserId, "Organiser");

        var result = await controller.Create(Request(enrolment.EnrolmentId));

        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task MyResults_ReturnsOnlyPublishedResultsWithTotalFinishers()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var me = TestHelpers.AddUser(context, "me@test.com");
        var rival = TestHelpers.AddUser(context, "rival@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId, "Published Race", -10);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var hiddenEvent = TestHelpers.AddEvent(context, organiser.UserId, "Hidden Race", -20);
        var hiddenEc = TestHelpers.AddEventCategory(context, hiddenEvent.EventId, category.CategoryId);

        var mine = TestHelpers.AddEnrolment(context, ev, ec, me, EnrolmentStatus.Confirmed, 1001);
        var theirs = TestHelpers.AddEnrolment(context, ev, ec, rival, EnrolmentStatus.Confirmed, 1002);
        var unpublished = TestHelpers.AddEnrolment(context, hiddenEvent, hiddenEc, me, EnrolmentStatus.Confirmed, 1001);

        context.Results.AddRange(
            new Result { EnrolmentId = mine.EnrolmentId, FinishTime = new TimeSpan(0, 52, 0), Position = 2, OverallRank = 2, Verified = true },
            new Result { EnrolmentId = theirs.EnrolmentId, FinishTime = new TimeSpan(0, 49, 0), Position = 1, OverallRank = 1, Verified = true },
            new Result { EnrolmentId = unpublished.EnrolmentId, FinishTime = new TimeSpan(0, 58, 0), Position = 5, OverallRank = 5, Verified = false });
        context.SaveChanges();

        var controller = TestHelpers.WithSession(new ResultsController(context), me.UserId, "Participant");

        var result = await controller.MyResults();

        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsAssignableFrom<IEnumerable<MyResultDto>>(ok.Value).ToList();
        var single = Assert.Single(list);
        Assert.Equal("Published Race", single.EventName);
        Assert.Equal(2, single.Position);
        Assert.Equal(2, single.TotalFinishers);
    }

    [Fact]
    public async Task Update_ChangesFinishTimeAndPublishedFlag()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        var existing = new Result { EnrolmentId = enrolment.EnrolmentId, FinishTime = new TimeSpan(0, 45, 0), Position = 1, OverallRank = 1, Verified = false };
        context.Results.Add(existing);
        context.SaveChanges();
        var controller = TestHelpers.WithSession(new ResultsController(context), organiser.UserId, "Organiser");

        var result = await controller.Update(existing.ResultId, new ResultRequestDto
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = new TimeSpan(0, 47, 30),
            Position = 2,
            Verified = true
        });

        Assert.IsType<OkObjectResult>(result);
        var saved = context.Results.Single();
        Assert.Equal(new TimeSpan(0, 47, 30), saved.FinishTime);
        Assert.Equal(2, saved.Position);
        Assert.True(saved.Verified);
    }
}
