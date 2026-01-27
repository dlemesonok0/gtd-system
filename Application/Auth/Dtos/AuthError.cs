namespace Application.Auth.Dtos;

public sealed record AuthError(AuthErrorCode ErrorCode, string Message);
