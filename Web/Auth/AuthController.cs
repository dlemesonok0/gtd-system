using System.Security.Claims;
using Application.Auth;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Domain.Auth.LoginRequest;
using RegisterRequest = Domain.Auth.RegisterRequest;

namespace Web.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthApiResponse>> RegisterAsync([FromBody] RegisterRequest reg)
    {
        var res = await service.RegisterAsync(reg);

        if (!res.IsSuccess || res.Value is null)
        {
            if (res.Error is not null && res.Error.ErrorCode == AuthErrorCode.EmailAlreadyInUse ||
                res.Error is not null && res.Error.ErrorCode == AuthErrorCode.PasswordTooShort)
            {
                return Conflict(new ProblemDetails { Title = res.Error.Message });
            }

            return BadRequest();
        }

        SetRefreshCookie(res.Value.RefreshToken);

        return Ok(new AuthApiResponse(res.Value.AccessToken));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthApiResponse>> LoginAsync([FromBody] LoginRequest login)
    {
        var res = await service.LoginAsync(login);

        if (!res.IsSuccess || res.Value is null)
        {
            if (res.Error is not null && res.Error.ErrorCode == AuthErrorCode.InvalidCredentials)
            {
                return Unauthorized(new ProblemDetails { Title = res.Error.Message });
            }

            return BadRequest();
        }

        SetRefreshCookie(res.Value.RefreshToken);

        return Ok(new AuthApiResponse(res.Value.AccessToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthApiResponse>> RefreshAsync()
    {
        if (Request.Cookies.TryGetValue("refresh_token", out var token) || string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        var res = await service.RefreshAsync(token);

        if (!res.IsSuccess || res.Value is null)
        {
            if (res.Error is not null && res.Error.ErrorCode == AuthErrorCode.RefreshTokenCantRotated)
            {
                return Unauthorized(new ProblemDetails { Title = res.Error.Message });
            }

            return BadRequest();
        }

        SetRefreshCookie(res.Value.RefreshToken);

        return Ok(new AuthApiResponse(res.Value.AccessToken));
    }


    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            await service.LogoutAsync(Guid.Parse((User.FindFirstValue("sub") ??
                                                  User.FindFirstValue(ClaimTypes.NameIdentifier))!));
        }
        catch
        {
            return BadRequest();
        }

        DeleteRefreshToken();

        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponse>> MeAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (userId is null || email is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse( Guid.Parse(userId), email ));
    }

    private void SetRefreshCookie(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            Response.Cookies.Append(
                "refresh_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = false,
                    Path = "/"
                });
        }
    }

    private void DeleteRefreshToken()
    {
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            Path = "/"
        });
    }
}
