using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Infrastructure.Data;
using MiniCommerce.UserService.Infrastructure.Repositories;
using MiniCommerce.UserService.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Configuration (appsettings)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

// DbContext
var pg = builder.Configuration.GetConnectionString("Postgres") ?? "Host=localhost;Port=5432;Database=userdb;Username=postgres;Password=postgres";
services.AddDbContext<UserDbContext>(opts => opts.UseNpgsql(pg));

// DI - repositories & services
services.AddScoped<MiniCommerce.UserService.Domain.Interfaces.IUserRepository, UserRepository>();
services.AddScoped<IAuthService, AuthService>();

// JWT settings
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
services.AddSingleton(jwtSettings);
services.AddSingleton<JwtTokenGenerator>();

// Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set true in production
    options.SaveToken = true;
    options.MapInboundClaims = false; //keep original claim names from JWT

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// Add controllers & swagger
services.AddControllers();
services.AddEndpointsApiExplorer();

// ? Add Swagger configuration with JWT support
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiniCommerce User Service API",
        Version = "v1",
        Description = "Handles user registration, login, and authentication."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text box below.\r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Build
var app = builder.Build();

// Migrate DB on startup (simple approach)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

// Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
