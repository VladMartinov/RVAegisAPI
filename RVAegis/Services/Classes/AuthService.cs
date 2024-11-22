using RVAegis.Contexts;
using RVAegis.Models.Auth;
using RVAegis.Models.UserModels;
using RVAegis.Services.Interfaces;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RVAegis.Models.AuthModels;
using Microsoft.EntityFrameworkCore;

namespace RVAegis.Services.Classes
{
    public class AuthService(ApplicationContext applicationContext, IConfiguration configuration) : IAuthService
    {
        private readonly ApplicationContext applicationContext = applicationContext;
        private readonly IConfiguration configuration = configuration;

        /* Метод по авториазции пользователя */
        public async Task<LoginResponse> Login(LoginRequest user)
        {
            var response = new LoginResponse();
            var identityUser = applicationContext.Users.FirstOrDefault(u => u.Login == user.Login);

            if (
                identityUser == null
                || identityUser.UserStatusId == (uint)UserStatusEnum.Blocked
                || identityUser.UserStatusId == (uint)UserStatusEnum.Removed
                || !BCrypt.Net.BCrypt.Verify(user.Password, identityUser.Password)
                ) return response;

            response.IsLoggedIn = true;

            response.IsLoggedIn = true;
            response.JwtToken = this.GenerateTokenString(identityUser.Login, identityUser.UserRoleId.ToString(), identityUser.UserStatusId.ToString());
            response.RefreshToken = GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await applicationContext.SaveChangesAsync();

            response.User = identityUser;

            return response;
        }

        /* Метод по генирации токена обновления */
        public async Task<LoginResponse> RefreshToken(TokenRequest model)
        {
            var principal = this.GetTokenPrincipal(model.JwtToken);

            var response = new LoginResponse();
            if (principal?.Identity?.Name is null)
                return response;

            var identityUser = await applicationContext.Users
                .Include(u => u.UserRole)
                .Include(u => u.UserStatus)
                .FirstOrDefaultAsync(u => u.Login == principal.Identity.Name);

            if (
                identityUser is null
                || identityUser.UserStatusId == (uint)UserStatusEnum.Blocked
                || identityUser.UserStatusId == (uint)UserStatusEnum.Removed
                || identityUser.RefreshToken != model.RefreshToken
                || identityUser.RefreshTokenExpiry < DateTime.UtcNow
                )
                return response;

            var role = identityUser.UserRole.ToString();
            var status = identityUser.UserStatus.ToString();

            if (role == null || status == null) return response;

            response.IsLoggedIn = true;
            response.JwtToken = GenerateTokenString(identityUser.Login, role, status);
            response.RefreshToken = GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await applicationContext.SaveChangesAsync();

            return response;
        }

        /* Метод по получению контекста безопасности */
        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt:Key").Value ?? ""));

            var validationParameters = new TokenValidationParameters()
            {
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = securityKey
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }

        /* Метод по генерации токена обновления */
        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
                numberGenerator.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);
            return refreshToken;
        }

        /* Метод по генерации токена доступа */
        private string GenerateTokenString(string login, string role, string status)
        {
            var claims = new List<Claim>()
            {
                new(type: ClaimTypes.Name, value: login ?? ""),
                new(type: ClaimTypes.Role, value: role ?? ""),
                new(type: "Status", value: status ?? ""),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt:Key").Value ?? ""));
            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToker = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    issuer: configuration.GetSection("Jwt:Issuer").Value,
                    audience: configuration.GetSection("Jwt:Audience").Value,
                    signingCredentials: signingCred
                    );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToker);
            return tokenString;
        }
    }
}
