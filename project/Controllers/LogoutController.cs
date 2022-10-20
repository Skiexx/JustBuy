using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustBuyApi.Controllers;

// Доступен только авторизованным пользователям [Authorize]
/// <inheritdoc />
[ApiController]
[Route("/logout")]
public class LogoutController : ControllerBase
{
    // API: /logout - выход из аккаунта
    /// <summary>
    /// Выход из аккаунта.
    /// </summary>
    /// <returns>Сообщение об успешном выходе</returns>
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