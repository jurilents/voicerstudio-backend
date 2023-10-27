// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Tokens;
// using NeerCore.DependencyInjection;
// using VoicerStudio.Database.Context;
// using VoicerStudio.Database.Entities;
// using VoicerStudio.Database.Options;
// using VoicerStudio.TelegramBot.Bot.Models;
//
// namespace VoicerStudio.TelegramBot.Bot.Core;
//
// [Service]
// public sealed class AccessTokenGenerator
// {
//     private readonly AppDbContext _dbContext;
//     private readonly SecurityOptions _options;
//
//     public AccessTokenGenerator(IOptions<SecurityOptions> optionsAccessor, AppDbContext dbContext)
//     {
//         _dbContext = dbContext;
//         _options = optionsAccessor.Value;
//     }
//
//     public async Task<JwtToken> GenerateAsync(AppUser user, CancellationToken ct = default)
//     {
//         var expires = DateTime.UtcNow.Add(_options.TokenLifetime);
//
//         var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.JwtSecret));
//         var tokenDescriptor = new SecurityTokenDescriptor
//         {
//             Subject = new ClaimsIdentity(await GetUserClaimsAsync(user, ct)),
//             SigningCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature),
//             IssuedAt = DateTime.UtcNow,
//             Expires = expires,
//         };
//
//         var tokenHandler = new JwtSecurityTokenHandler();
//         SecurityToken jwt = tokenHandler.CreateToken(tokenDescriptor);
//
//         return new JwtToken(tokenHandler.WriteToken(jwt), expires);
//     }
//
//     private async Task<IEnumerable<Claim>> GetUserClaimsAsync(AppUser user, CancellationToken ct)
//     {
//         var claims = new List<Claim>
//         {
//             new(Claims.Id, user.Id.ToString()),
//             new(Claims.Username, user.UserName),
//             new(Claims.Image, user.ProfilePhotoUrl ?? ""),
//             new(Claims.FirstName, user.FirstName),
//         };
//
//         IEnumerable<string> roles = await GetUserRolesAsync(user.Id, ct);
//
//         claims.AddRange(roles.Select(role => new Claim(Claims.Role, role)));
//
//         claims.AddRange(await GetUserClaimsAsync(user.Id, ct));
//         return claims;
//     }
//
//     private async Task<IEnumerable<Claim>> GetUserClaimsAsync(long userId, CancellationToken ct)
//     {
//         // TODO: mb smth not works
//         List<Claim> claims = await (from u in _dbContext.Set<AppUserClaim>()
//                 where u.UserId == userId
//                 select new Claim(u.ClaimType, u.ClaimValue ?? "null"))
//             .ToListAsync(ct);
//
//         claims.AddRange(await _dbContext.Set<AppUserRole>()
//             .Where(e => e.UserId == userId)
//             .Join(_dbContext.Set<AppRoleClaim>(), ur => ur.RoleId, rc => rc.RoleId, (_, rc) => rc)
//             .Select(e => new Claim(e.ClaimType!, e.ClaimValue ?? "null"))
//             .ToListAsync(ct));
//
//         return claims;
//     }
//
//     private Task<List<string>> GetUserRolesAsync(long userId, CancellationToken ct)
//     {
//         return (from u in _dbContext.Set<AppUserRole>()
//             where u.UserId == userId
//             join r in _dbContext.Set<AppRole>() on u.RoleId equals r.Id
//             select r.Name).ToListAsync(ct);
//     }
// }