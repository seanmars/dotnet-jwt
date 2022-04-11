namespace WebApp.Configuration;

public class JwtConfiguration
{
    public string Issuer { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
}