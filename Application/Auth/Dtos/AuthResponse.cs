namespace Domain.Auth;

public sealed record AuthResponse(string AccessToken, string RefreshToken);
