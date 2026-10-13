using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Models;
using Xunit;

public class EnrolmentsControllerTests
{
    private RaceDayDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task Enrol_WhenCategoryFull_ReturnsConflict()
    {
        var context = GetContext();
        var ec = new EventCategory { EventCategoryId = 1, EventId = 1, CategoryId = 1, MaxParticipants = 1, CurrentParticipants = 1 };
        context.EventCategories.Add(ec);
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserId", 5);

        var controller = new EnrolmentsController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.Enrol(1);
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Enrol_WhenAlreadyEnrolled_ReturnsConflict()
    {
        var context = GetContext();
        var ec = new EventCategory { EventCategoryId = 2, EventId = 1, CategoryId = 1, MaxParticipants = 10 };
        context.EventCategories.Add(ec);
        context.Enrolments.Add(new Enrolment { ParticipantId = 5, EventCategoryId = 2 });
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserId", 5);

        var controller = new EnrolmentsController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.Enrol(2);
        Assert.IsType<ConflictObjectResult>(result);
    }
}