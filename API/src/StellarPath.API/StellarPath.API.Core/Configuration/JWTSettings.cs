namespace StellarPath.API.Core.Configuration;

public  class JWTSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 1;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
