using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Data;
using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Filters;
using ST10335260_PROG6212_POE_Part2.Services;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly RaceDayDbContext _context;
    public UsersController(RaceDayDbContext context) => _context = context;

    [HttpGet("me")]
    [RequireAuth]
    public async Task<IActionResult> GetMe()
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null) return NotFound();

        return Ok(new
        {
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.ProfilePictureUrl,
            Role = user.Role!.RoleName
        });
    }

    [HttpPut("me")]
    [RequireAuth]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == email && u.UserId != user.UserId))
            return Conflict(new { message = "That email address is already in use." });

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = email;

        await _context.SaveChangesAsync();
        return Ok(new { user.UserId, user.Email, user.FirstName, user.LastName, user.ProfilePictureUrl });
    }

    [HttpPost("me/profile-picture")]
    [RequireAuth]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture([FromForm] ImageUploadDto dto, [FromServices] IBlobStorageService storage)
    {
        var problem = ImageValidator.Validate(dto.File);
        if (problem is not null) return BadRequest(new { message = problem });

        var user = await _context.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.ProfilePictureUrl = await storage.UploadAsync(BlobContainers.ProfilePictures, dto.File);
        await _context.SaveChangesAsync();

        return Ok(new UploadResultDto { Url = user.ProfilePictureUrl });
    }
}
