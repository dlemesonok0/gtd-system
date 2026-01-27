using Application.Auth;
using Application.Auth.Dtos;
using Application.Shared;
using Domain.Auth;

namespace Application.UseCases;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenStore refreshTokenStore) : IAuthService
{
    public async Task<Result<AuthResponse, AuthError>> RegisterAsync(RegisterRequest reg)
    {
        var email = reg.Email.Trim();
        if (await userRepository.FindByEmailAsync(email) != null)
            return Result<AuthResponse, AuthError>.Failure(new AuthError(AuthErrorCode.EmailAlreadyInUse,
                "The email already exists."));

        if (reg.Password.Length < 8)
        {
            return Result<AuthResponse, AuthError>.Failure(new AuthError(AuthErrorCode.PasswordTooShort,
                "Need password longer than 7 characters."));
        }

        var user = new ApplicationUser
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(reg.Password)
        };

        await userRepository.CreateAsync(user);

        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);


        return Result<AuthResponse, AuthError>.Success(new AuthResponse(access, refresh));
    }

    public async Task<Result<AuthResponse, AuthError>> LoginAsync(LoginRequest login)
    {
        var user = await userRepository.FindByEmailAsync(login.Email);
        if (user == null || !passwordHasher.Verify(user.PasswordHash, login.Password))
            return Result<AuthResponse, AuthError>.Failure(new AuthError(AuthErrorCode.InvalidCredentials,
                "Check credentials."));

        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);
        return Result<AuthResponse, AuthError>.Success(new AuthResponse(access, refresh));
    }

    public async Task<Result<AuthResponse, AuthError>> RefreshAsync(string refreshToken)
    {
        var userId = await refreshTokenStore.ValidateAndRotateAsync(refreshToken);

        if (userId is null)
            return Result<AuthResponse, AuthError>.Failure(new AuthError(AuthErrorCode.InvalidCredentials,
                "This refresh token is invalid."));

        var id = userId.Value;

        var user = await userRepository.FindByIdAsync(id);

        if (user is null)
        {
            return Result<AuthResponse, AuthError>.Failure(new AuthError(AuthErrorCode.InvalidCredentials,
                "This refresh token is invalid."));
        }

        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);
        return Result<AuthResponse, AuthError>.Success(new AuthResponse(access, refresh));
    }

    public Task LogoutAsync(Guid userId)
    {
        return refreshTokenStore.RevokeAsync(userId);
    }
}
