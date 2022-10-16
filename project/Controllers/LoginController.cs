using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using project.Data;

namespace JustBuyApi.Controllers;

[ApiController]
[Route("login")]
public class LoginController : ControllerBase
{
    private readonly ProjectContext _context;

    public class LoginUser 
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public LoginController(ProjectContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
    {
        User? user =
            await _context.Users.FirstOrDefaultAsync(
                u => u.Email == loginUser.Email && u.Password == loginUser.Password);
        
        if (user == null)
        {
            // Пользователь не найден
            return Problem("Authentication failed", statusCode: 401);
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };
        var jwt = new JwtSecurityToken(
            issuer: AuthOption.ISSUER,
            audience: AuthOption.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
            signingCredentials: new SigningCredentials(AuthOption.GetSymmetricSecurityKey(), 
                SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return Ok(new {user_token = encodedJwt});
    }
}