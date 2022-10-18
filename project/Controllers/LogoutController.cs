using JustBuyApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustBuyApi.Controllers;

// Доступен только авторизованным пользователям [Authorize]
[ApiController]
[Route("/logout")]
public class LogoutController : ControllerBase
{
    private readonly ProjectContext _context;

    public LogoutController(ProjectContext context)
    {
        _context = context;
    }
    
    // API: /logout - выход из аккаунта
    [HttpGet]
    [Authorize]
    [Produces("application/json")]
    public Task<IActionResult> Logout()
    {
        // Возвращаем статус 200
        return Task.FromResult<IActionResult>(new ContentResult
        {
            Content = @"{ ""data"": { ""message"": ""logout"" } }",
            ContentType = "application/json",
            StatusCode = 200
        });
    }
}