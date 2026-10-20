using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using EventsApp.API.Middleware;
using EventsApp.Application.Abstractions;
using EventsApp.Application.Mapping;
using EventsApp.Application.Repositories;
using EventsApp.Application.Services;
using EventsApp.Application.Settings;
using EventsApp.Infrastructure.Data;
using EventsApp.Infrastructure.Repositories;
using EventsApp.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;                                    // תוסף NLog ל-ASP.NET Core

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();                    // מבטל את ספקי הלוג המובנים, כדי ש-NLog יהיה היחיד
builder.Host.UseNLog();                             // כל ILogger באפליקציה כותב דרך NLog (קונסול וקובץ), לפי nlog.config

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SeedSettings>(builder.Configuration.GetSection(SeedSettings.SectionName));

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. Set it in User Secrets or an environment variable.");

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();                  // שמירה אחת
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();  // מאגר לוח
builder.Services.AddScoped<IEventRepository, EventRepository>();        // מאגר אירועים
builder.Services.AddScoped<ICalendarService, CalendarService>();        // שירות לוח
builder.Services.AddScoped<IEventService, EventService>();              // שירות אירועים
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();          // מאגר ספקים
builder.Services.AddScoped<IEventItemRepository, EventItemRepository>();        // מאגר סעיפים
builder.Services.AddScoped<ICandidacyRepository, CandidacyRepository>();        // מאגר מועמדויות
builder.Services.AddScoped<IProviderService, ProviderService>();                // שירות ספקים
builder.Services.AddScoped<ICandidacyService, CandidacyService>();              // שירות מועמדויות
builder.Services.AddScoped<IDeletionRequestRepository, DeletionRequestRepository>();    // מאגר בקשות מחיקה
builder.Services.AddScoped<IDeletionRequestService, DeletionRequestService>();          // שירות בקשות מחיקה
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();  // מאגר התראות
builder.Services.AddScoped<INotificationService, NotificationService>();        // שירות התראות
builder.Services.AddScoped<IStatsRepository, StatsRepository>();                // מאגר ספירות
builder.Services.AddScoped<IStatsService, StatsService>();                      // שירות ספירות
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventsApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "הדבק כאן את הטוקן בלבד (בלי המילה Bearer)"
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

var app = builder.Build();

var hasConnectionString = !string.IsNullOrWhiteSpace(
    builder.Configuration.GetConnectionString("DefaultConnection"));

if (hasConnectionString)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
}
else
{
    app.Logger.LogWarning("No connection string configured. Skipping migrations and seed.");
}

app.UseMiddleware<ExceptionMiddleware>();          // ראשון ב-pipeline: תופס חריגה מכל שלב שאחריו ומחזיר ProblemDetails
app.UseMiddleware<CorrelationIdMiddleware>();      // קובע מזהה לבקשה, כותב לוג כניסה וסיום, ומחזיר את המזהה בכותרת

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ReactClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { app = "EventsApp API", status = "running" }));
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
