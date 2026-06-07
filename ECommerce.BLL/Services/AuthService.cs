using System.Threading.Tasks;
using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Identity;
using ECommerce.DAL.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Auth Service: Handles Registration and Login
    /// 
    /// لماذا:
    /// - نستخدم Microsoft Identity للـ Password Hashing, Validation, User Store
    /// - JWT Token: نعمل Token بعد الـ Login عشان الـ API تبقى stateless
    /// - Token يحتوي على UserId (NameIdentifier) و Email و Role
    /// - userId يتم استخراجه من Claims في كل API endpoint (مش بيتبعت في الـ Request)
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<Result<string>> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<string>.Failure("Email is already registered.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address
            };

            // Create user with hashed password (Identity handles this automatically)
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<string>.Failure("Registration failed.", errors);
            }

            // Assign default "User" role
            await _userManager.AddToRoleAsync(user, Common.Constants.RoleNames.User);

            // Generate JWT token
            var token = GenerateJwtToken(user, await _userManager.GetRolesAsync(user));

            return Result<string>.Success(token, "Registration successful.");
        }

        public async Task<Result<string>> LoginAsync(LoginRequest request)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Result<string>.Failure("Invalid email or password.");

            // Check password
            var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValidPassword)
                return Result<string>.Failure("Invalid email or password.");

            // Get roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = GenerateJwtToken(user, roles);

            return Result<string>.Success(token, "Login successful.");
        }

        public async Task<Result<string>> GetRoleByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result<string>.Failure("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return Result<string>.Success(role);
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            // Add all roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationInMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
