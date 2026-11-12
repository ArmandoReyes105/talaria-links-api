using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using talaria_links_api.Common;
using talaria_links_api.Entities;
using talaria_links_api.Middleware;
using talaria_links_api.Options;
using talaria_links_api.Persistence;
using talaria_links_api.Services.Implementations;
using talaria_links_api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<talaria_links_api.Options.LinkOptions>()
    .Bind(builder.Configuration.GetSection(talaria_links_api.Options.LinkOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies[AuthCookie.Name];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

//Cors configuracion
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(o => o.AddPolicy("web", p => p
    .WithOrigins(allowedOrigins)
    .AllowCredentials()
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddFixedWindowLimiter("auth", w =>
    {
        w.PermitLimit = 10;
        w.Window = TimeSpan.FromMinutes(1);
    });
});

builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

builder.Services
    .AddScoped<ITokenService, TokenService>()
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<IShortUrlService, ShortUrlService>()
    .AddScoped<IAccountService, AccountService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

await AdminSeeder.SeedAsync(app.Services, app.Configuration);

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("web");

app.UseRateLimiter();

app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllers();

app.Run();
