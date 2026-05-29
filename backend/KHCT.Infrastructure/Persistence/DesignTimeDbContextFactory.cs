using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KHCT.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KhctDbContext>
{
    public KhctDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("KHCT_CONNECTION")
            ?? "Server=localhost;Port=3306;Database=khct;User=root;Password=rootpass;SslMode=None;AllowPublicKeyRetrieval=True;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));
        var options = new DbContextOptionsBuilder<KhctDbContext>()
            .UseMySql(connectionString, serverVersion, b => b.MigrationsAssembly(typeof(KhctDbContext).Assembly.FullName))
            .Options;
        return new KhctDbContext(options);
    }
}
