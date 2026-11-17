using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;
using Xunit;

public class EventsControllerTests
{
    private static EventRequestDto ValidRequest() => new()
    {
        EventName = "Spring Fun Run",
        Description = "A friendly run",
        EventDate = DateTime.UtcNow.AddDays(20),
        Location = "Durban",
        DistanceKm = 5,
        EventType = EventType.Run
    };

    [Fact]
    public async Task Create_AssignsCurrentOrganiserAsOwner()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var controller = TestHelpers.WithSession(new EventsController(context), organiser.UserId, "Organiser");

        var result = await controller.Create(ValidRequest());

        Assert.IsType<CreatedAtActionResult>(result);
        var saved = Assert.Single(context.Events);
        Assert.Equal(organiser.UserId, saved.OrganiserId);
    }

    [Fact]
    public async Task Update_ByDifferentOrganiser_Returns403()
    {
        var context = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(context, "owner@test.com", "Organiser");
        var other = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var ev = TestHelpers.AddEvent(context, owner.UserId);
        var controller = TestHelpers.WithSession(new EventsController(context), other.UserId, "Organiser");

        var result = await controller.Update(ev.EventId, ValidRequest());

        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenEventHasEnrolments_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new EventsController(context), organiser.UserId, "Organiser");

        var result = await controller.Delete(ev.EventId);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Single(context.Events);
    }

    [Fact]
    public async Task Delete_OwnEventWithoutEnrolments_ReturnsNoContent()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var controller = TestHelpers.WithSession(new EventsController(context), organiser.UserId, "Organiser");

        var result = await controller.Delete(ev.EventId);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Events);
    }

    [Fact]
    public async Task GetAll_WithUpcomingFilter_ExcludesPastEvents()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        TestHelpers.AddEvent(context, organiser.UserId, "Future Run", 15);
        TestHelpers.AddEvent(context, organiser.UserId, "Old Run", -15);
        var controller = TestHelpers.WithSession(new EventsController(context), null, null);

        var result = await controller.GetAll(new EventFilter { Upcoming = true });

        var ok = Assert.IsType<OkObjectResult>(result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventSummaryDto>>(ok.Value).ToList();
        Assert.Single(events);
        Assert.Equal("Future Run", events[0].EventName);
    }

    [Fact]
    public async Task GetAll_FilteredByType_ReturnsOnlyMatchingType()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        TestHelpers.AddEvent(context, organiser.UserId, "Road Run", 10, EventType.Run);
        TestHelpers.AddEvent(context, organiser.UserId, "Big Ride", 10, EventType.Cycle);
        var controller = TestHelpers.WithSession(new EventsController(context), null, null);

        var result = await controller.GetAll(new EventFilter { Type = EventType.Cycle });

        var ok = Assert.IsType<OkObjectResult>(result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventSummaryDto>>(ok.Value).ToList();
        Assert.Single(events);
        Assert.Equal("Big Ride", events[0].EventName);
    }

    [Fact]
    public async Task UploadBanner_WithValidImage_StoresUrlOnEvent()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var storage = new FakeBlobStorage();
        var controller = TestHelpers.WithSession(new EventsController(context), organiser.UserId, "Organiser");

        var result = await controller.UploadBanner(ev.EventId, new ImageUploadDto { File = TestHelpers.NewImage() }, storage);

        Assert.IsType<OkObjectResult>(result);
        Assert.Single(storage.Uploads);
        Assert.Equal(storage.Uploads[0], context.Events.Single().BannerUrl);
    }

    [Fact]
    public async Task UploadBanner_WithNonImageFile_ReturnsBadRequest()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var storage = new FakeBlobStorage();
        var controller = TestHelpers.WithSession(new EventsController(context), organiser.UserId, "Organiser");

        var result = await controller.UploadBanner(ev.EventId, new ImageUploadDto { File = TestHelpers.NewImage("notes.txt", "text/plain") }, storage);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(storage.Uploads);
    }

    [Fact]
    public async Task UploadBanner_ByDifferentOrganiser_Returns403()
    {
        var context = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(context, "owner@test.com", "Organiser");
        var other = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var ev = TestHelpers.AddEvent(context, owner.UserId);
        var controller = TestHelpers.WithSession(new EventsController(context), other.UserId, "Organiser");

        var result = await controller.UploadBanner(ev.EventId, new ImageUploadDto { File = TestHelpers.NewImage() }, new FakeBlobStorage());

        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
    }
}
