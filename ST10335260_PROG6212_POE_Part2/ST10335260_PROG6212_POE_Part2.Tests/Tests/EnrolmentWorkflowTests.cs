using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;
using Xunit;

public class EnrolmentWorkflowTests
{
    [Fact]
    public async Task Enrol_WithOpenCategory_CreatesPendingEnrolmentWithBibNumber()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), participant.UserId, "Participant");

        var result = await controller.Enrol(ec.EventCategoryId);

        var created = Assert.IsType<CreatedResult>(result);
        var dto = Assert.IsType<EnrolmentDto>(created.Value);
        Assert.Equal(EnrolmentStatus.Pending, dto.Status);
        Assert.Equal(1001, dto.BibNumber);
        Assert.Equal(1, context.EventCategories.Single().CurrentParticipants);
    }

    [Fact]
    public async Task Enrol_ForEventInThePast_ReturnsConflict()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId, "Old Race", -5);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), participant.UserId, "Participant");

        var result = await controller.Enrol(ec.EventCategoryId);

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Empty(context.Enrolments);
    }

    [Fact]
    public async Task UpdateStatus_ByOwningOrganiser_ConfirmsEnrolment()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId, 100, 1);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant, EnrolmentStatus.Pending);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), organiser.UserId, "Organiser");

        var result = await controller.UpdateStatus(enrolment.EnrolmentId, new UpdateEnrolmentStatusDto { Status = EnrolmentStatus.Confirmed });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(EnrolmentStatus.Confirmed, context.Enrolments.Single().Status);
    }

    [Fact]
    public async Task UpdateStatus_ByDifferentOrganiser_Returns403()
    {
        var context = TestHelpers.NewContext();
        var owner = TestHelpers.AddUser(context, "owner@test.com", "Organiser");
        var other = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, owner.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant, EnrolmentStatus.Pending);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), other.UserId, "Organiser");

        var result = await controller.UpdateStatus(enrolment.EnrolmentId, new UpdateEnrolmentStatusDto { Status = EnrolmentStatus.Confirmed });

        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Equal(EnrolmentStatus.Pending, context.Enrolments.Single().Status);
    }

    [Fact]
    public async Task UpdateStatus_ToCancelled_FreesUpPlace()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId, 100, 1);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), organiser.UserId, "Organiser");

        await controller.UpdateStatus(enrolment.EnrolmentId, new UpdateEnrolmentStatusDto { Status = EnrolmentStatus.Cancelled });

        Assert.Equal(0, context.EventCategories.Single().CurrentParticipants);
    }

    [Fact]
    public async Task Cancel_ByEnrolledParticipant_RemovesEnrolmentAndFreesPlace()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId, 100, 1);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, participant);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), participant.UserId, "Participant");

        var result = await controller.Cancel(enrolment.EnrolmentId);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Enrolments);
        Assert.Equal(0, context.EventCategories.Single().CurrentParticipants);
    }

    [Fact]
    public async Task Cancel_SomeoneElsesEnrolment_Returns403()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var owner = TestHelpers.AddUser(context, "owner@test.com");
        var intruder = TestHelpers.AddUser(context, "intruder@test.com");
        var category = TestHelpers.AddCategory(context);
        var ev = TestHelpers.AddEvent(context, organiser.UserId);
        var ec = TestHelpers.AddEventCategory(context, ev.EventId, category.CategoryId);
        var enrolment = TestHelpers.AddEnrolment(context, ev, ec, owner);
        var controller = TestHelpers.WithSession(new EnrolmentsController(context), intruder.UserId, "Participant");

        var result = await controller.Cancel(enrolment.EnrolmentId);

        Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
        Assert.Single(context.Enrolments);
    }

    [Fact]
    public async Task Dashboard_ReturnsCountsForOrganisersOwnEvents()
    {
        var context = TestHelpers.NewContext();
        var organiser = TestHelpers.AddUser(context, "org@test.com", "Organiser");
        var otherOrganiser = TestHelpers.AddUser(context, "other@test.com", "Organiser");
        var participant = TestHelpers.AddUser(context, "p@test.com");
        var second = TestHelpers.AddUser(context, "p2@test.com");
        var category = TestHelpers.AddCategory(context);

        var upcoming = TestHelpers.AddEvent(context, organiser.UserId, "Upcoming", 14);
        var past = TestHelpers.AddEvent(context, organiser.UserId, "Past", -14);
        var foreign = TestHelpers.AddEvent(context, otherOrganiser.UserId, "Foreign", 14);

        var upcomingEc = TestHelpers.AddEventCategory(context, upcoming.EventId, category.CategoryId);
        var pastEc = TestHelpers.AddEventCategory(context, past.EventId, category.CategoryId);
        var foreignEc = TestHelpers.AddEventCategory(context, foreign.EventId, category.CategoryId);

        TestHelpers.AddEnrolment(context, upcoming, upcomingEc, participant, EnrolmentStatus.Pending, 1001);
        TestHelpers.AddEnrolment(context, upcoming, upcomingEc, second, EnrolmentStatus.Confirmed, 1002);
        TestHelpers.AddEnrolment(context, past, pastEc, participant, EnrolmentStatus.Confirmed, 1001);
        TestHelpers.AddEnrolment(context, foreign, foreignEc, participant, EnrolmentStatus.Confirmed, 1001);

        var controller = TestHelpers.WithSession(new OrganiserController(context), organiser.UserId, "Organiser");

        var result = await controller.Dashboard();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dashboard = Assert.IsType<DashboardDto>(ok.Value);
        Assert.Equal(2, dashboard.TotalEvents);
        Assert.Equal(3, dashboard.TotalEnrolments);
        Assert.Equal(1, dashboard.UpcomingEvents);
        Assert.Equal(1, dashboard.PendingEnrolments);
    }
}
