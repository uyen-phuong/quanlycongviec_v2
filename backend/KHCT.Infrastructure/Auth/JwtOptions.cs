namespace KHCT.Infrastructure.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "khct-api";
    public string Audience { get; set; } = "khct-web";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
