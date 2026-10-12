using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.Filters;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public UsersController(RaceDayDbContext context) => _context = context;

    [HttpGet("me")]
    [RequireAuth]
    public async Task<IActionResult> GetMe()
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null) return NotFound();

        return Ok(new { user.UserId, user.Email, user.FirstName, user.LastName, Role = user.Role!.RoleName });
    }

    [HttpPut("me")]
    [RequireAuth]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId")!.Value;
        var user = await _context.Users.FindAsync(userId);
        if (user is null) return NotFound();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;

        await _context.SaveChangesAsync();
        return Ok(new { user.UserId, user.Email, user.FirstName, user.LastName });
    }
}

public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}