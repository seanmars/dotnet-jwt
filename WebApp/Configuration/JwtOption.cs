using Microsoft.Extensions.Options;

namespace WebApp.Configuration;

public class JwtOption
{
    public const string IssueKeyName = "Jwt:Issuer";
    public const string SecretKeyName = "Jwt:Secret";
    
    public string Issuer { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}