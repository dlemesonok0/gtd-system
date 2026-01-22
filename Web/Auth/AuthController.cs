using System.Security.Claims;
using Domain.Auth;
using gtd_system.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Domain.Auth.LoginRequest;
using RegisterRequest = Domain.Auth.RegisterRequest;

namespace Web.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService service) : ControllerBase
{
    [HttpPost]
    [Route("/register")]
    public async Task<AuthResponse> RegisterAsync(RegisterRequest reg)
    {
        return await service.RegisterAsync(reg);
    }

    [HttpPost]
    [Route("/login")]
    public async Task<AuthResponse> LoginAsync(LoginRequest login)
    {
        return await service.LoginAsync(login);
    }

    [HttpPost]
    [Route("/refresh")]
    public async Task<AuthResponse> RefreshAsync(string refresh)
    {
        return await service.RefreshAsync(refresh);
    }

    
    [HttpGet]
    [Route("/logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await service.LogoutAsync(Guid.Parse((User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier))!));
        return Ok();
    }

    [HttpGet]
    [Route("/me")]
    [Authorize]
    public async Task<IActionResult> MeAsync()
    {
        var userId =  User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        return Ok(new { userId, email });
    }
}
