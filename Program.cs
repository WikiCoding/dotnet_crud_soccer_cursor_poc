using System.Text;
using System.Threading.RateLimiting;
using cursor_dotnet_test.Middleware;
using Microsoft.AspNetCore.RateLimiting;
using cursor_dotnet_test.Persistence;
using cursor_dotnet_test.Persistence.Repositories;
using StackExchange.Redis;
using cursor_dotnet_test.Services;
using cursor_dotnet_test.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<SoccerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SoccerDb")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<SoccerDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("players", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromSeconds(10);
        limiterOptions.QueueLimit = 2;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect($"{builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"},abortConnect=false"));
builder.Services.Configure<RedisCacheSettings>(builder.Configuration.GetSection("RedisCache"));
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

builder.Services.AddScoped<TeamsService>();
builder.Services.AddScoped<ICreateTeam>(sp => sp.GetRequiredService<TeamsService>());
builder.Services.AddScoped<IGetTeamById>(sp => sp.GetRequiredService<TeamsService>());
builder.Services.AddScoped<IGetAllTeams>(sp => sp.GetRequiredService<TeamsService>());
builder.Services.AddScoped<IUpdateTeam>(sp => sp.GetRequiredService<TeamsService>());
builder.Services.AddScoped<IDeleteTeam>(sp => sp.GetRequiredService<TeamsService>());

builder.Services.AddScoped<PlayersService>();
builder.Services.AddScoped<ICreatePlayer>(sp => sp.GetRequiredService<PlayersService>());
builder.Services.AddScoped<IGetPlayerById>(sp => sp.GetRequiredService<PlayersService>());
builder.Services.AddScoped<IGetPlayersByTeam>(sp => sp.GetRequiredService<PlayersService>());
builder.Services.AddScoped<IUpdatePlayer>(sp => sp.GetRequiredService<PlayersService>());
builder.Services.AddScoped<IDeletePlayer>(sp => sp.GetRequiredService<PlayersService>());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("SoccerDb")!,
        name: "postgresql",
        tags: new[] { "ready", "live" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis",
        tags: new[] { "ready", "live" });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SoccerDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var db = scope.ServiceProvider.GetRequiredService<SoccerDbContext>();
        logger.LogInformation("Development shutdown: dropping database for a fresh start on next run");
        db.Database.EnsureDeleted();
    });
}

app.UseExceptionHandler();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/healthz/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapControllers();

app.Run();
