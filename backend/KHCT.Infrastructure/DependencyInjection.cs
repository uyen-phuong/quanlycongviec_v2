using KHCT.Application.Common.Interfaces;
using KHCT.Application.PersonalEvaluations;
using KHCT.Infrastructure.Auth;
using KHCT.Infrastructure.Excel;
using KHCT.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KHCT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Server=localhost;Port=3306;Database=khct;User=root;Password=rootpass;SslMode=None;AllowPublicKeyRetrieval=True;";
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));

        services.AddDbContext<KhctDbContext>(options =>
            options.UseMySql(connectionString, serverVersion,
                b => b.MigrationsAssembly(typeof(KhctDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<KhctDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IMainPlanExcelImportService, MainPlanExcelImportService>();
        services.AddScoped<IMainPlanExcelExportService, MainPlanExcelExportService>();
        services.AddScoped<IPersonalEvaluationExportService, PersonalEvaluationExportService>();
        services.Configure<AttachmentStorageOptions>(configuration.GetSection("Storage"));
        services.AddScoped<IAttachmentStorage, LocalAttachmentStorage>();

        return services;
    }
}
