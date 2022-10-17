using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JustBuyApi.Controllers;

[Route("signup")]
[ApiController]
public class SignUpController : ControllerBase
{
    private readonly ProjectContext _context;

    public SignUpController(ProjectContext context)
    {
        _context = context;
    }
    
    // Класс пользователя с полями для регистрации
    public class SignUpUser
    {
        public string? fio { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    
    // API: /signup - регистрация пользователя
    [HttpPost]
    [Produces("application/json")]
    public async Task<IActionResult> PostSignUp([FromBody] SignUpUser signUpUser)
    {
        // Словарь ошибок, где ключ - название поля, а значение - ошибка
        var errors = new Dictionary<string, string>();
        
        // Проверка ФИО на пустоту
        if (string.IsNullOrWhiteSpace(signUpUser.fio))
        {
            errors.Add("fio", "[ \"Fio is required\" ]");
        }

        // Проверка Email на пустоту, валидность и уникальность
        if (string.IsNullOrWhiteSpace(signUpUser.Email))
        {
            errors.Add("email", "[ \"Email is required\" ]");
        }
        else if (!new EmailAddressAttribute().IsValid(signUpUser.Email))
        {
            errors.Add("email", "[ \"Email is invalid\" ]");
        }
        else if (await _context.Users.AnyAsync(u => u.Email == signUpUser.Email))
        {
            errors.Add("email", "[ \"Email is already taken\" ]");
        }

        // Проверка пароля на пустоту и длину
        if (string.IsNullOrWhiteSpace(signUpUser.Password))
        {
            errors.Add("password", "[ \"Password is required\" ]");
        }
        else if (signUpUser.Password?.Length < 6)
        {
            errors.Add("password", "[ \"Password must be at least 6 characters\" ]");
        }

        // Если есть ошибки, то возвращаем их
        if (errors.Count > 0)
        {
            return new ContentResult
            {
                Content =
                    $@"{{""error"": {{ ""code"": 422, ""message"": ""Validation error"", ""errors"": {{ {string.Join(", ", errors.Select(x => $"\"{x.Key}\": {x.Value}"))} }} }} }}",
                ContentType = "application/json",
                StatusCode = 422
            };
        }

        // Создаем нового пользователя
        var user = new User
        {
            Email = signUpUser.Email,
            Password = signUpUser.Password,
            FullName = signUpUser.fio,
            RoleId = 2
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

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
            StatusCode = 201
        };
    }
}