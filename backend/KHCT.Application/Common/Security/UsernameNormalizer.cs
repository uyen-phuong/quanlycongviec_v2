namespace KHCT.Application.Common.Security;

public static class UsernameNormalizer
{
    public static string Normalize(string username) =>
        username.Trim().ToLowerInvariant();
}
