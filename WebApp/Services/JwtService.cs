using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.Services;

public class JwtService
{
    public const string JwtIssueKey = "Jwt:Issuer";
    public const string JwtSecretKey = "Jwt:Secret";

    private readonly ILogger<JwtService> _logger;
    private readonly IConfiguration _configuration;

    public JwtService(ILogger<JwtService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public static byte[] GetKeyBytes(string secretKey)
    {
        return Encoding.UTF8.GetBytes(secretKey);
    }

    public string GenerateToken(string userName, int expireMinutes = 30)
    {
        var jwtIssuer = _configuration[JwtIssueKey];
        var jwtSecret = _configuration[JwtSecretKey];

        var symmetricKey = GetKeyBytes(jwtSecret);
        var now = DateTime.UtcNow;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Iss, jwtIssuer),
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);

        return token;
    }
}