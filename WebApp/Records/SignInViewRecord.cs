using System.Text.Json.Serialization;

namespace WebApp.Records;

public record SignInViewRecord
{
    public SignInViewRecord(string username, string password)
    {
        Username = username;
        Password = password;
    }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }
}