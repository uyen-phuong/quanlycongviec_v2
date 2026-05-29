using KHCT.Application.Common.Interfaces;

namespace KHCT.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

    public bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}
