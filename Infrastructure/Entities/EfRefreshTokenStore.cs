using System.Security.Cryptography;
using System.Text;
using Domain.Auth;
using Infrastructure.Data;
using Infrastructure.Jwt;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Entities;

public class EfRefreshTokenStore(AppDbContext context, JwtOptions jwtOptions) : IRefreshTokenStore
{
    private static string CreateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string Sha256(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
    
    public async Task<string> IssueAsync(Guid userId)
    {
        var token = CreateSecureToken();
        var hash = Sha256(token);

        context.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        });

        await context.SaveChangesAsync();
        return token;
    }

    public async Task<Guid?> ValidateAndRotateAsync(string refreshToken)
    {
        var hash = Sha256(refreshToken);
        var rToken = context.RefreshTokens.FirstOrDefault(t => t.TokenHash == hash);
        if (rToken == null) return null;
        if (rToken.RevokedOn is not null) return null;
        if (rToken.ExpiresOn < DateTimeOffset.UtcNow) return null;
        
        rToken.RevokedOn = DateTimeOffset.UtcNow;
        
        var newToken = CreateSecureToken();
        var newHash = Sha256(newToken);

        var newRToken = new RefreshTokenEntity
        {
            TokenHash = newHash,
            UserId = rToken.UserId,
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenDays),
        };
        
        context.RefreshTokens.Add(newRToken);
        
        await context.SaveChangesAsync();
        
        return newRToken.UserId;
    }

    public async Task RevokeAsync(Guid userId)
    {
        var tokens = await context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedOn == null).ToListAsync();
        
        var now =  DateTimeOffset.UtcNow;
        foreach (var token in tokens) token.RevokedOn = now;

        await context.SaveChangesAsync();
    }
}
