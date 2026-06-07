using Microsoft.AspNetCore.Identity;
using ECommerce.DAL.Entities;

namespace ECommerce.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register JWT Authentication
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var key = config["Jwt:SecretKey"] ?? throw new Exception("JWT SecretKey is required in appsettings.json");
            var issuer = config["Jwt:Issuer"] ?? "ECommerceAPI";
            var audience = config["Jwt:Audience"] ?? "ECommerceUsers";

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key))
                };
            });

            return services;
        }
    }

    public static class RoleManagerExtensions
    {
        /// <summary>
        /// Seed default roles on startup
        /// </summary>
        public static async Task SeedRolesAsync(this RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
