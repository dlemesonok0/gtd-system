namespace Domain.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user);
}
