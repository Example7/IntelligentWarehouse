namespace MobileApi.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "IntelligentWarehouse.MobileApi";
    public string Audience { get; set; } = "IntelligentWarehouse.MobileClient";
    public string Key { get; set; } = "CHANGE_ME_TO_A_LONG_RANDOM_SECRET_KEY_32+";
    public int ExpirationMinutes { get; set; } = 60;
}
