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
    public async Task<IActionResult> LoginAsync(LoginRequest login)
    {
        var res = await service.LoginAsync(login);
        
        Response.Cookies.Append(
            "refresh_token", 
            res.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                Path = "auth/refresh"
            });
        
        return Ok(res.AccessToken);
    }

    [HttpPost]
    [Route("/refresh")]
    public async Task<IActionResult> RefreshAsync(HttpContext ctx, string refresh)
    {
        if (!ctx.Request.Cookies.TryGetValue("refresh_token", out var token) || !string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var res = await service.RefreshAsync(refresh);

        if (!string.IsNullOrWhiteSpace(res.RefreshToken))
        {
            Response.Cookies.Append(
                "refresh_token", 
                res.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = false,
                    Path = "auth/refresh"
                });
        }
        
        return Ok(res.AccessToken);
    }

    
    [HttpGet]
    [Route("/logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(HttpContext ctx)
    {
        await service.LogoutAsync(Guid.Parse((User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier))!));
        ctx.Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "auth/refresh"
        });
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
