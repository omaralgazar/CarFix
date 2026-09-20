using System.Text;
using CarFix.API.Middleware;
using CarFix.Application.Configuration;
using CarFix.Application.Interfaces;
using CarFix.Application.Interfaces.IRepositories;
using CarFix.Application.Services;
using CarFix.Infrastructure.Persistence;
using CarFix.Infrastructure.Persistence.Repositories;
using CarFix.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace CarFix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. إعداد قاعدة البيانات PostgreSQL
            builder.Services.AddDbContext<CarFixDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // 2. إعداد إعدادات JWT
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

            // 3. تسجيل الـ Repositories (Dependency Injection)
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IServiceCenterRepository, ServiceCenterRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // 4. تسجيل الـ Application Services
            builder.Services.AddScoped<ServiceCenterService>();
            builder.Services.AddScoped<VehicleService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // 5. تسجيل أدوات الأمان والتشفير
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

            // 6. إعداد التوثيق JWT Authentication
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtSettings = jwtSection.Get<JwtSettings>()
                ?? throw new InvalidOperationException("JWT settings not found in appsettings.json");

            if (string.IsNullOrEmpty(jwtSettings.Secret))
            {
                throw new InvalidOperationException("JWT Secret key undefined");
            }

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
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            // 7. إعداد توثيق OpenAPI ودعم JWT لـ Scalar UI
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "CarFix API";
                    document.Info.Version = "v1";

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "JWT Authorization header using the Bearer scheme."
                    };

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes["Bearer"] = securityScheme;

                    var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
                    var securityRequirement = new OpenApiSecurityRequirement
                    {
                        [schemeReference] = new List<string>()
                    };

                    document.Security ??= new List<OpenApiSecurityRequirement>();
                    document.Security.Add(securityRequirement);

                    return Task.CompletedTask;
                });
            });

            // 8. بناء التطبيق
            var app = builder.Build();

            // 9. إعداد الـ Middleware Pipeline
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("CarFix API Reference")
                        .WithTheme(ScalarTheme.Purple);
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}