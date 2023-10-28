using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeerCore.DependencyInjection;
using VoicerStudio.Database.Context;
using VoicerStudio.Database.Entities;
using VoicerStudio.Database.Options;
using VoicerStudio.Database.Repositories;

namespace VoicerStudio.Database.Infrastructure;

[Service]
internal class TokenRepository : ITokenRepository
{
    private readonly AppDbContext _dbContext;
    private readonly SecurityOptions _securityOptions;

    public TokenRepository(AppDbContext dbContext, IOptions<SecurityOptions> securityOptionsAccessor)
    {
        _dbContext = dbContext;
        _securityOptions = securityOptionsAccessor.Value;
    }


    public async Task<AppToken> GenerateAsync(Guid userId, CancellationToken ct)
    {
        var reuseBeforeExpiration = DateTime.UtcNow.AddHours(-12);
        if (await _dbContext.Tokens.AnyAsync(x => x.UserId == userId && x.Expired > reuseBeforeExpiration, ct))
            return new AppToken
            {
                UserToken = null,
                JwtToken = null,
                Expired = default
            };
        // throw new InvalidOperationException("User already has a token");

        var secureRandomString = GenerateSecureRandomString();
        var token = new AppToken
        {
            UserToken = secureRandomString,
            JwtToken = "",
            UserId = userId,
            Expired = DateTime.UtcNow.Add(_securityOptions.TokenLifetime),
            Created = DateTime.UtcNow,
        };

        await _dbContext.Tokens.AddAsync(token, ct);
        await _dbContext.SaveChangesAsync(ct);

        return token;
    }

    private string GenerateSecureRandomString()
    {
        var secureString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_securityOptions.UserTokenLength));
        return secureString.Replace('/', 'o').Replace('+', '0').Replace("=", "").ToUpper();
    }
}