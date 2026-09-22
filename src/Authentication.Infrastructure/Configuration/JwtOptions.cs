namespace Authentication.Infrastructure.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; }
}