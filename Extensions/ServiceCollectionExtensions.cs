// Extensions/ApplicationServiceExtensions.cs
using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Repositories;
using Calendar.API.Services;
using Calendar.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Calendar.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// 註冊應用程式服務
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 註冊資料庫上下文
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // 註冊 Repositories
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<ITodoRepository, TodoRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSettingRepository, UserSettingRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // 註冊 Services
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserSettingService, UserSettingService>();

            // 註冊 AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // 註冊密碼哈希工具
            services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }

        /// <summary>
        /// 配置 JWT 認證
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));

            services.AddAuthentication(options =>
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
                    ValidIssuer = jwtSettings["Issuer"] ?? "DefaultIssuer",
                    ValidAudience = jwtSettings["Audience"] ?? "DefaultAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }

        /// <summary>
        /// 配置 CORS 策略
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }
    }
}