using Domain.Auth;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Helpers;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();


    public string Hash(string password)
    {
        return _hasher.HashPassword(new object(), password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var res = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return res is not PasswordVerificationResult.Failed;
    }
}
