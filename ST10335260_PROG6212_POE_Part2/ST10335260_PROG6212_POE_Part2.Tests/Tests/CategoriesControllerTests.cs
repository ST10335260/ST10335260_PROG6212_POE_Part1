using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.DTOs;
using Xunit;

public class CategoriesControllerTests
{
    [Fact]
    public async Task AddToEvent_WhenCategoryAlreadyAdded_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var controller = TestHelpers.WithSession(new CategoriesController(context), organiser.UserId, "Organiser");

        var result = await controller.AddToEvent(ev.EventId, new EventCategoryRequestDto { CategoryId = category.CategoryId, EntryFee = 150, MaxParticipants = 50 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task AddToEvent_ForAnotherOrganisersEvent_Returns403()
    {
        var context = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(context, "owner@test.com", "Organiser");
        var other = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, owner.UserId);
        var controller = TestHelpers.WithSession(new CategoriesController(context), other.UserId, "Organiser");

        var result = await controller.AddToEvent(ev.EventId, new EventCategoryRequestDto { CategoryId = category.CategoryId, EntryFee = 150, MaxParticipants = 50 });

        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task AddToEvent_WithValidData_CreatesEventCategory()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var controller = TestHelpers.WithSession(new CategoriesController(context), organiser.UserId, "Organiser");

        var result = await controller.AddToEvent(ev.EventId, new EventCategoryRequestDto { CategoryId = category.CategoryId, EntryFee = 150, MaxParticipants = 50 });

        Assert.IsType<CreatedAtActionResult>(result);
        var saved = Assert.Single(context.EventCategories);
        Assert.Equal(150, saved.EntryFee);
        Assert.Equal(50, saved.MaxParticipants);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        TestHelpers.AddCategory(context, "10km Road Run");
        var controller = TestHelpers.WithSession(new CategoriesController(context), organiser.UserId, "Organiser");

        var result = await controller.CreateCategory(new CategoryRequestDto { CategoryName = "10km Road Run", DefaultDistance = 10 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenCategoryHasEnrolments_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new CategoriesController(context), organiser.UserId, "Organiser");

        var result = await controller.Delete(ec.EventCategoryId);

        Assert.IsType<ConflictObjectResult>(result);
    }
}
