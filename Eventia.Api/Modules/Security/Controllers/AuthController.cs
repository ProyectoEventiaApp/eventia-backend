using Microsoft.AspNetCore.Mvc;
using Modules.Security.Application.Auth;

namespace Modules.Security.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthAppService _auth;
    public AuthController(IAuthAppService auth) => _auth = auth;

    public record LoginRequest(string Email, string Password); 

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var (token, user, roles, permissions) = await _auth.LoginAsync(req.Email, req.Password);

        var dto = new UserDto(user.Id, user.Name, user.Email, user.IsActive, roles, permissions);

        return Ok(new { token, user = dto });
    }

    public record UserDto(int Id, string Name, string Email, bool IsActive, List<string> Roles, List<string> Permissions);
}
