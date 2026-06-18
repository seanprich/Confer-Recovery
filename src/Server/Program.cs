using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using Serilog;
using ConferRecovery.Server.Configuration;
using ConferRecovery.Server.Middleware;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Services;
using ConferRecovery.Server.Telemetry;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File("logs/confer-.log", rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30));

    // ── Configuration ─────────────────────────────────────────────────────────
    var mongoSettings = builder.Configuration.GetSection("MongoDb").Get<MongoDbSettings>()
        ?? throw new InvalidOperationException("MongoDb configuration is required.");
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt configuration is required.");
    var liveKitSettings = builder.Configuration.GetSection("LiveKit").Get<LiveKitSettings>()
        ?? new LiveKitSettings();

    builder.Services.AddSingleton(mongoSettings);
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddSingleton(liveKitSettings);

    // ── MongoDB ───────────────────────────────────────────────────────────────
    builder.Services.AddSingleton<IMongoClient>(_ =>
        new MongoClient(mongoSettings.ConnectionString));
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

    // ── Data Protection (encrypts LiveKit secrets at rest in MongoDB) ─────────
    builder.Services.AddDataProtection();

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
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

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
