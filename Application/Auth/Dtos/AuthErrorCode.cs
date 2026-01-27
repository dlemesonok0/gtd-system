namespace Application.Auth;

public enum AuthErrorCode
{
    EmailAlreadyInUse,
    PasswordTooShort,
    InvalidCredentials,
    RefreshTokenCantRotated,
}
