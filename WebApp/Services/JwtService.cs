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

    public string GenerateToken(string userName, int expireMinutes = 30)
    {
        var jwtIssuer = _jwtOption.Issuer;
        var jwtSecret = _jwtOption.Secret;

        var symmetricKey = GetKeyBytes(jwtSecret);
        var now = DateTime.UtcNow;

        var userClaimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        });

        // TODO: add roles to claims

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = userClaimsIdentity,
            Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}