using System.Text;
using BusinessLayer.Commands.Users;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using BusinessLayer.Service.PasswordHandler;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware.Service;
using NLog;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using NLog.Web;
NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
NLog.Common.InternalLogger.LogToConsole = true;


var builder = WebApplication.CreateBuilder(args);

var keyDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".aspnet",
    "Fundoo-OIDC-Test-Keys");

Directory.CreateDirectory(keyDirectory);

builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services
    .AddDataProtection()
    .SetApplicationName("FundooOIDCTestV2")
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fundoo API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();
builder.Services.AddScoped<INotesBL, NotesBL>();
builder.Services.AddScoped<INotesRL, NotesRL>();
builder.Services.AddScoped<IEmailService ,EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasher<UserEntity>>();
builder.Services.AddScoped<EmailClient>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(RegisterUserCommand).Assembly));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:ConnectionString"];

    options.InstanceName = "Fundoo:";
});


builder.Services.AddDbContext<FundooContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("FundooConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

const string ExternalCookieScheme = "ExternalCookie";
const string GoogleOidcScheme = "GoogleOIDC";

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing.");

var googleClientId =
    builder.Configuration["Authentication:Google:ClientId"]
    ?? throw new InvalidOperationException("Google ClientId is missing.");

var googleClientSecret =
    builder.Configuration["Authentication:Google:ClientSecret"]
    ?? throw new InvalidOperationException("Google ClientSecret is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    })
    .AddCookie(ExternalCookieScheme, options =>
    {
        options.Cookie.Name = "Fundoo.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = false;
    })
    .AddOpenIdConnect(GoogleOidcScheme, options =>
    {
        options.SignInScheme = ExternalCookieScheme;

        options.Authority = "https://accounts.google.com";
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;

        options.CallbackPath = "/signin-oidc";
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.UsePkce = true;

        options.SaveTokens = false;
        options.GetClaimsFromUserInfoEndpoint = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

builder.Services.AddAuthorization();

var app = builder.Build();
//temp
app.MapGet("/test-redis", async (IDistributedCache cache) =>
{
    await cache.SetStringAsync("test-string-key", "Redis is working 2!");
    await cache.SetStringAsync(
        "user:101:name",
        "Harshdeep");
    await cache.SetStringAsync("user:102:name", "Keshav");
    string? value = await cache.GetStringAsync("test-key");
   

    return Results.Ok(new { message = value });
});
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();