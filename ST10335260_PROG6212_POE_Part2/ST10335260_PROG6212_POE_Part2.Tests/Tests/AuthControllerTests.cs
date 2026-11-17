using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Controllers;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public class AuthControllerTests
{
    private RaceDayDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new RaceDayDbContext(options);
        context.Database.EnsureCreated();

        if (!context.Roles.Any())
        {
            context.Roles.Add(new Role { RoleId = 1, RoleName = "Organiser" });
            context.Roles.Add(new Role { RoleId = 2, RoleName = "Participant" });
            context.SaveChanges();
        }

        return context;
    }

    [Fact]
    public async Task Register_WithValidData_CreatesUser()
    {
        var context = GetContext();
        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterDto
        {
            Email = "test@example.com",
            Password = "Password123",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = new DateTime(2000, 1, 1),
            Role = "Participant"
        });

        Assert.IsType<CreatedResult>(result);
        Assert.Equal(1, context.Users.Count());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var context = GetContext();
        context.Users.Add(new User { Email = "dup@example.com", PasswordHash = "x", RoleId = 2 });
        context.SaveChanges();

        var controller = new AuthController(context);
        var result = await controller.Register(new RegisterDto
        {
            Email = "dup@example.com",
            Password = "x",
            Role = "Participant"
        });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var context = GetContext();
        context.Users.Add(new User
        {
            Email = "a@b.com",
            PasswordHash = ST10335260_PROG6212_POE_Part2.Services.PasswordHasher.Hash("correct"),
            RoleId = 2
        });
        context.SaveChanges();

        var controller = new AuthController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            }
        };

        var result = await controller.Login(new LoginDto { Email = "a@b.com", Password = "wrong" });
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_StoresUserIdAndRoleInSession()
    {
        var context = GetContext();
        var user = new User
        {
            Email = "ok@test.com",
            PasswordHash = ST10335260_PROG6212_POE_Part2.Services.PasswordHasher.Hash("Secret123"),
            RoleId = 1
        };
        context.Users.Add(user);
        context.SaveChanges();

        var session = new TestSession();
        var controller = new AuthController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            }
        };

        var result = await controller.Login(new LoginDto { Email = "ok@test.com", Password = "Secret123" });

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user.UserId, session.GetInt32("UserId"));
        Assert.Equal("Organiser", session.GetString("Role"));
    }

    [Fact]
    public void Logout_ClearsTheSession()
    {
        var context = GetContext();
        var session = new TestSession();
        session.SetInt32("UserId", 7);
        session.SetString("Role", "Participant");

        var controller = new AuthController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            }
        };

        var result = controller.Logout();

        Assert.IsType<OkObjectResult>(result);
        Assert.Null(session.GetInt32("UserId"));
        Assert.Null(session.GetString("Role"));
    }

    [Fact]
    public async Task Register_WithUnknownRole_ReturnsBadRequest()
    {
        var context = GetContext();
        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterDto
        {
            Email = "new@test.com",
            Password = "Password123",
            FirstName = "New",
            LastName = "User",
            DateOfBirth = new DateTime(1999, 5, 5),
            Role = "Admin"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(context.Users);
    }

    [Fact]
    public async Task Register_WithFutureDateOfBirth_ReturnsBadRequest()
    {
        var context = GetContext();
        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterDto
        {
            Email = "future@test.com",
            Password = "Password123",
            FirstName = "Future",
            LastName = "Person",
            DateOfBirth = DateTime.UtcNow.AddYears(1),
            Role = "Participant"
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
