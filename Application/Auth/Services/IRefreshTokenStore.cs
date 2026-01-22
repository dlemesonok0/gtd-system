namespace Domain.Auth;

public interface IRefreshTokenStore
{
    Task<string> IssueAsync(Guid userId);
    Task<Guid?> ValidateAndRotateAsync(string refreshToken);
    Task RevokeAsync(Guid userId);
}
