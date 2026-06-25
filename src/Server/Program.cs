using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using Serilog;
using ConferRecovery.Server.Configuration;
using ConferRecovery.Security.Extensions;
using ConferRecovery.Server.Middleware;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Seeding;
using ConferRecovery.Server.Services;
using ConferRecovery.Server.Telemetry;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    LoadDotEnvAndBridgeVariables();

    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options =>
        options.Limits.MaxRequestBodySize = 64 * 1024);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/confer-.log", rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // ── Configuration ─────────────────────────────────────────────────────────
    var mongoSection = builder.Configuration.GetSection("MongoDb");
    var mongoSettings = new MongoDbSettings
    {
        ConnectionString = mongoSection["ConnectionString"] ?? "mongodb://localhost:27017",
        DatabaseName = mongoSection["DatabaseName"] ?? "confer",
        Username = mongoSection["Username"]
            ?? Environment.GetEnvironmentVariable("MongoDb__Username")
            ?? Environment.GetEnvironmentVariable("MONGO_USERNAME"),
        Password = mongoSection["Password"]
            ?? Environment.GetEnvironmentVariable("MongoDb__Password")
            ?? Environment.GetEnvironmentVariable("MONGO_PASSWORD")
    };

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtSettings = new JwtSettings
    {
        SecretKey = jwtSection["SecretKey"]
            ?? Environment.GetEnvironmentVariable("Jwt__SecretKey")
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? string.Empty,
        Issuer = jwtSection["Issuer"] ?? "confer",
        Audience = jwtSection["Audience"] ?? "confer-clients",
        ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes)
            ? expiryMinutes
            : 60
    };

    if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
        throw new InvalidOperationException(
            "Jwt:SecretKey must be at least 32 bytes (256 bits) for HMAC-SHA256.");
    var liveKitSettings = builder.Configuration.GetSection("LiveKit").Get<LiveKitSettings>()
        ?? new LiveKitSettings();

    builder.Services.AddSingleton(mongoSettings);
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddSingleton(liveKitSettings);

    // ── MongoDB ───────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMongoClient>(_ =>
    {
        if (mongoSettings.HasCredentials)
        {
            var credential = MongoCredential.CreateCredential(
                "admin", mongoSettings.Username, mongoSettings.Password);
            var clientSettings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
            clientSettings.Credential = credential;
            return new MongoClient(clientSettings);
        }
        return new MongoClient(mongoSettings.ConnectionString);
    });
    builder.Services.AddSingleton<IMongoDatabase>(sp =>
        sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

    // ── Repositories ──────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMemberRepository, MemberRepository>();
    builder.Services.AddSingleton<IChapterRepository, ChapterRepository>();
    builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
    builder.Services.AddSingleton<IAuditRepository, AuditRepository>();

    // ── Telemetry ─────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IConferMetrics, ConferMetrics>();

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddMeter(ConferMetrics.MeterName)
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter());

    builder.Services.AddHealthChecks()
        .AddCheck<MongoHealthCheck>("mongodb", tags: ["ready"]);

    // ── Services ──────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMemberService, MemberService>();
    builder.Services.AddSingleton<IChapterService, ChapterService>();
    builder.Services.AddSingleton<IRoomService, RoomService>();
    builder.Services.AddSingleton<ILiveKitTokenService, LiveKitTokenService>();
    builder.Services.AddSingleton<IAuditService, AuditService>();
    builder.Services.AddSingleton<IApiTokenService, ApiTokenService>();

    // ── Security ──────────────────────────────────────────────────────────────
    builder.Services.AddConferSecurity();

    // ── Database Seeder ───────────────────────────────────────────────────────
    var seedSettings = builder.Configuration.GetSection("Seed").Get<SeedSettings>() ?? new SeedSettings();
    builder.Services.AddSingleton(seedSettings);
    builder.Services.AddSingleton<DatabaseSeeder>();

    // ── Data Protection (encrypts LiveKit secrets at rest in MongoDB) ─────────
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Environment.GetEnvironmentVariable("DP_KEYS_PATH") ?? "/app/keys"))
        .SetApplicationName("ConferRecovery");

    // ── Authentication ────────────────────────────────────────────────────────
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
            };
        });

    builder.Services.AddAuthorization();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var origins = builder.Configuration
                .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (origins.Length > 0)
                policy.WithOrigins(origins)
                    .WithHeaders("Authorization", "Content-Type")
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE");
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApi(options =>
    {
        options.AddOperationTransformer((operation, context, _) =>
        {
            if (context.Description.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return Task.CompletedTask;

            operation.OperationId = string.IsNullOrWhiteSpace(actionDescriptor.AttributeRouteInfo?.Name)
                ? $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}"
                : actionDescriptor.AttributeRouteInfo!.Name!;

            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    app.UseHttpsRedirection();
    app.UseConferSecurity();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<AuditMiddleware>();

    app.MapControllers();
    app.MapPrometheusScrapingEndpoint("/metrics").AllowAnonymous();
    app.MapHealthChecks("/healthz").AllowAnonymous();
    app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    }).AllowAnonymous();

    await app.Services.GetRequiredService<DatabaseSeeder>().SeedAsync();

    Log.Information("ConferRecovery Server starting on {Env}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static void LoadDotEnvAndBridgeVariables()
{
    var dotEnvPath = FindDotEnvPath();
    if (dotEnvPath is null)
    {
        return;
    }

    foreach (var line in File.ReadAllLines(dotEnvPath, Encoding.UTF8))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
        {
            continue;
        }

        var splitIndex = trimmed.IndexOf('=');
        if (splitIndex <= 0)
        {
            continue;
        }

        var key = trimmed[..splitIndex].Trim();
        var value = trimmed[(splitIndex + 1)..].Trim();

        if ((value.StartsWith('"') && value.EndsWith('"')) ||
            (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            value = value[1..^1];
        }

        SetEnvIfMissing(key, value);
    }

    SetEnvIfMissing("MongoDb__Username", Environment.GetEnvironmentVariable("MONGO_USERNAME"));
    SetEnvIfMissing("MongoDb__Password", Environment.GetEnvironmentVariable("MONGO_PASSWORD"));
    SetEnvIfMissing("Jwt__SecretKey", Environment.GetEnvironmentVariable("JWT_SECRET_KEY"));
}

static string? FindDotEnvPath()
{
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (current is not null)
    {
        var candidate = Path.Combine(current.FullName, ".env");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        current = current.Parent;
    }

    return null;
}

static void SetEnvIfMissing(string key, string? value)
{
    if (string.IsNullOrWhiteSpace(key) || value is null)
    {
        return;
    }

    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
    {
        Environment.SetEnvironmentVariable(key, value);
    }
}
