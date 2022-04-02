using System.Text.Json.Serialization;

namespace WebApp.Records;

public record SignUpViewRecord
{
    public SignUpViewRecord(string email, string username, string password, string confirmPassword)
    {
        Email = email;
        Username = username;
        Password = password;
        ConfirmPassword = confirmPassword;
    }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; }
}