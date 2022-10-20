using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JustBuyApi.Controllers;

/// <inheritdoc />
[ApiController]
[Route("login")]
public class LoginController : ControllerBase
{
    private readonly ProjectContext _context;
    
    //  Класс с полями для входа
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class LoginUser 
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
#pragma warning restore CS1591
    /// <inheritdoc />
    public LoginController(ProjectContext context)
    {
        _context = context;
    }

    // API: /login - Авторизация
    /// <summary>
    /// Авторизация
    /// </summary>
    /// <param name="loginUser">Поля необходимые для авторизации</param>
    /// <returns> JWT токен </returns>
    /// <response code="401">Пользователь не найден</response>
    /// <response code="200">Токен в теле ответа</response>
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorsController.ErrorClass), 401)]
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