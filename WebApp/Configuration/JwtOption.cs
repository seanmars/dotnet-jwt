using Microsoft.Extensions.Options;

namespace WebApp.Configuration;

public class JwtOption
{
    public string Issuer { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}