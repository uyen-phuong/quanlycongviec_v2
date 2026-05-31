using System.Text;
using KHCT.Api.Auth;
using KHCT.Api.Middleware;
using KHCT.Application;
using KHCT.Application.Attachments;
using KHCT.Application.Common.Interfaces;
using KHCT.Infrastructure;
using KHCT.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = AttachmentSupport.MaxSizeBytes;
});

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/khct-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddControllers();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = AttachmentSupport.MaxSizeBytes;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KHCT API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Nhập JWT access token (không cần tiền tố 'Bearer ')"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddHttpContextAccessor();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins =
    [
        "http://localhost:5173",
        "http://127.0.0.1:5173"
    ];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.SetIsOriginAllowed(_ => false);
            return;
        }

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("KHCT")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

if (OperatingSystem.IsWindows())
{
    dataProtection.ProtectKeysWithDpapi();
}

builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string devJwtKey = "change-this-development-key-with-at-least-32-characters";
var jwtKey = builder.Configuration["Jwt:Key"] ?? devJwtKey;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "khct-api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "khct-web";

if (!builder.Environment.IsDevelopment() && jwtKey == devJwtKey)
{
    throw new InvalidOperationException(
        "FATAL: Jwt:Key chưa được cấu hình. Không được dùng key mặc định ở môi trường production.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => 
        policy.RequireRole(KHCT.Domain.Constants.RoleConstants.Admin));
    options.AddPolicy("RequireLanhDao", policy => 
        policy.RequireRole(
            KHCT.Domain.Constants.RoleConstants.Admin,
            KHCT.Domain.Constants.RoleConstants.TruongKtnb,
            KHCT.Domain.Constants.RoleConstants.PhoTruongKtnb));
    options.AddPolicy("RequireManager", policy => 
        policy.RequireRole(
            KHCT.Domain.Constants.RoleConstants.Admin,
            KHCT.Domain.Constants.RoleConstants.TruongKtnb,
            KHCT.Domain.Constants.RoleConstants.PhoTruongKtnb,
            KHCT.Domain.Constants.RoleConstants.TruongPhong,
            KHCT.Domain.Constants.RoleConstants.PhoPhong));
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("KHCT API started on {Urls}", string.Join(", ", app.Urls));
});

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { data = new { status = "ok" } }));

if (Environment.GetEnvironmentVariable("KHCT_AUTO_MIGRATE") == "true")
{
    await DbInitializer.InitializeAsync(app.Services);
}

app.Run();

public partial class Program;
