using Domain.Auth;

namespace gtd_system.UseCases;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenStore refreshTokenStore)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest reg)
    {
        var email = reg.Email.Trim();
        if (await userRepository.FindByEmailAsync(email) != null)
            throw new Exception("Email already exists");

        var user = new ApplicationUser
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(reg.Password)
        };

        await userRepository.CreateAsync(user);

        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);


        return new AuthResponse(access, refresh);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest login)
    {
        var user = await userRepository.FindByEmailAsync(login.Email);
        if (user == null)
            throw new Exception("User not found");

        if (!passwordHasher.Verify(user.PasswordHash, login.Password))
            throw new Exception("Wrong password");

        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);
        return new AuthResponse(access, refresh);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var userId = await refreshTokenStore.ValidateAndRotateAsync(refreshToken)
                     ?? throw new Exception("Refresh token could not be rotated");

        var user = await userRepository.FindByIdAsync(userId) ?? throw new Exception("User not found");
        
        var access = jwtTokenService.CreateAccessToken(user);
        var refresh = await refreshTokenStore.IssueAsync(user.Id);
        return new AuthResponse(access, refresh);
    }

    public Task LogoutAsync(Guid userId)
    {
        return refreshTokenStore.RevokeAsync(userId);
    }
}
