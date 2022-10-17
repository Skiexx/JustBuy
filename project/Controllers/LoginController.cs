using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JustBuyApi.Controllers;

[ApiController]
[Route("login")]
public class LoginController : ControllerBase
{
    private readonly ProjectContext _context;
    
    //  Класс с полями для входа
    public class LoginUser 
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public LoginController(ProjectContext context)
    {
        _context = context;
    }

    // API: /login - Авторизация
    [HttpPost]
    [Produces("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginUser loginUser)
    {
        // Поиск пользователя по email и паролю
        User? user =
            await _context.Users.FirstOrDefaultAsync(
                u => u.Email == loginUser.Email && u.Password == loginUser.Password);
        
        // Если пользователь не найден
        if (user == null)
        {
            // Возвращаем ошибку
            return new ContentResult
            {
                Content = "{\"error\": {\"code\": 401, \"message\": \"Authentication failed\"}}",
                ContentType = "application/json",
                StatusCode = 401
            };
        }
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, user.RoleId.ToString()),
            new("Id", user.Id.ToString())
        };
        
        // Создаем JWT-токен
        var jwt = new JwtSecurityToken(
            issuer: AuthOption.ISSUER,
            audience: AuthOption.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
            signingCredentials: new SigningCredentials(AuthOption.GetSymmetricSecurityKey(), 
                SecurityAlgorithms.HmacSha256));
        
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        
        // Возвращаем токен
        return new ContentResult
        {
            Content = $@"{{""data"": {{""user_token"": ""{encodedJwt}""}}}}", 
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}