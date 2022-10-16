using Microsoft.AspNetCore.Mvc;
using project.Data;

namespace JustBuyApi.Controllers
{
    [Route("signup")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly ProjectContext _context;

        public SignUpController(ProjectContext context)
        {
            _context = context;
        }
        //TODO: Add post for sign up
        // [HttpPost]
        // public async Task<IActionResult> PostSignUp([FromBody] User user)
        // {
        //     if (user.Email == null || user.Password == null || user.FullName == null)
        //     {
        //         return BadRequest();
        //     }
        // }
    }
}