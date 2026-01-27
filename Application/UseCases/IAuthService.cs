using Application.Auth.Dtos;
using Application.Shared;
using Domain.Auth;

namespace Application.UseCases;

public interface IAuthService
{
    public Task<Result<AuthResponse, AuthError>> RegisterAsync(RegisterRequest reg);

    public Task<Result<AuthResponse, AuthError>> LoginAsync(LoginRequest login);

    public Task<Result<AuthResponse, AuthError>> RefreshAsync(string refreshToken);

    public Task LogoutAsync(Guid userId);
}