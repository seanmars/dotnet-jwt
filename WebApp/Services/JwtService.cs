using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApp.Configuration;

namespace WebApp.Services;

public class JwtService
{
    public const string JwtIssueKey = "Jwt:Issuer";
    public const string JwtSecretKey = "Jwt:Secret";

    private readonly ILogger<JwtService> _logger;
    private readonly JwtOption _jwtOption;

    public JwtService(ILogger<JwtService> logger, IOptions<JwtOption> options)
    {
        _logger = logger;
        _jwtOption = options.Value;
    }

    public static byte[] GetKeyBytes(string secretKey)
    {
        return Encoding.UTF8.GetBytes(secretKey);
    }

    public string GenerateToken(string userName, int expireMinutes = 30, IList<Claim>? claims = null)
    {
        var jwtIssuer = _jwtOption.Issuer;
        var jwtSecret = _jwtOption.Secret;

        var userClaimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        });

        if (claims != null)
        {
            userClaimsIdentity.AddClaims(claims);
        }

        var now = DateTime.UtcNow;
        var symmetricKey = GetKeyBytes(jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            IssuedAt = now,
            Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),
            Subject = userClaimsIdentity,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}