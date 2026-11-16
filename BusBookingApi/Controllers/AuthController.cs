using System.Text;
using BusBookingApi.Data;
using BusBookingApi.DTOs;
using BusBookingApi.Models;
using BusBookingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    BusBookingDbContext db,
    JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthUserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthUserDto>> Register(RegisterRequest request)
    {
        if (Encoding.UTF8.GetByteCount(request.Password) > 72)
        {
            return BadRequest(new { message = "Password must not exceed 72 UTF-8 bytes." });
        }

        var userName = request.UserName.Trim();
        var emailId = request.EmailId.Trim();
        var fullName = request.FullName.Trim();
        var nameTaken = await db.Users.AnyAsync(user => user.UserName.ToLower() == userName.ToLower());
        var emailTaken = await db.Users.AnyAsync(user => user.EmailId.ToLower() == emailId.ToLower());

        if (nameTaken || emailTaken)
        {
            var errors = new Dictionary<string, string[]>();
            if (nameTaken) errors["userName"] = ["Username already taken"];
            if (emailTaken) errors["emailId"] = ["Email already registered"];
            return Conflict(new { errors });
        }

        var user = new AppUser
        {
            UserName = userName,
            EmailId = emailId,
            FullName = fullName,
            Role = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedDate = DateTime.UtcNow
        };

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var duplicateExists = await db.Users.AnyAsync(user =>
                user.UserName.ToLower() == userName.ToLower() ||
                user.EmailId.ToLower() == emailId.ToLower());
            if (!duplicateExists)
            {
                throw;
            }

            return Conflict(new { errors = new Dictionary<string, string[]> { ["userName"] = ["Username already taken"], ["emailId"] = ["Email already registered"] } });
        }

        return StatusCode(StatusCodes.Status201Created, new AuthUserDto(user.UserId, user.UserName, user.FullName));
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var userName = request.UserName.Trim();
        var user = await db.Users.SingleOrDefaultAsync(
            candidate => candidate.UserName.ToLower() == userName.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(new LoginResponse(
            tokenService.CreateToken(user),
            user.UserId,
            user.UserName,
            user.FullName));
    }
}
