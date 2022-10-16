using JustBuyApi.Data;
using Microsoft.AspNetCore.Mvc;

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

    public class SignUpUser
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    
    //TODO: Add post for sign up
    [HttpPost]
    public async Task<IActionResult> PostSignUp([FromBody] SignUpUser signUpUser)
    {
        if (string.IsNullOrEmpty(signUpUser.Email) ||
            string.IsNullOrEmpty(signUpUser.Password) ||
            string.IsNullOrEmpty(signUpUser.FullName))
        {
            return UnprocessableEntity();
        }

        return Ok();
    }
}