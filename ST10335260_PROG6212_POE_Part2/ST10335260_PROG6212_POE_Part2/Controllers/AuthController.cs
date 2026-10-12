using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public AuthController(RaceDayDbContext context) => _context = context;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { message = "Email already registered." });

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
        if (role is null)
            return BadRequest(new { message = "Role must be 'Organiser' or 'Participant'." });

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            RoleId = role.RoleId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created(string.Empty, new { user.UserId, user.Email, user.FirstName, user.LastName, Role = role.RoleName });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role!.RoleName);

        return Ok(new { user.UserId, user.Email, Role = user.Role.RoleName });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }
}