using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NeerCore.DependencyInjection;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Repositories;
using VoicerStudio.Database.Context;
using VoicerStudio.Database.Entities;

namespace VoicerStudio.Infrastructure.Repositories;

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
        var token = await _dbContext.Tokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Expired > reuseBeforeExpiration, ct);
        if (token is not null) return token;
        // throw new InvalidOperationException("User already has a token");

        var secureRandomString = GenerateSecureRandomString();
        token = new AppToken
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