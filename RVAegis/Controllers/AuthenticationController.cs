using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVAegis.Contexts;
using RVAegis.Models.Auth;
using RVAegis.Models.AuthModels;
using RVAegis.Models.HistoryModels;
using RVAegis.Services.Interfaces;
using System.Security.Cryptography;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController(ApplicationContext applicationContext, IConfiguration configuration, IAuthService authService, ILoggingService loggingService, IEmailService emailService) : Controller
    {
        private void AddCookie(string key, string value, DateTime expires)
        {
            HttpContext.Response.Cookies.Append(key, value,
                new CookieOptions
                {
                    Expires = expires,
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        private void DeleteCookie(string key)
        {
            HttpContext.Response.Cookies.Delete(key,
                new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.None
                });
        }

        // POST api/authentication/login
        /// <summary>
        /// Метод для авторизации пользователя
        /// </summary>
        /// <param name="userCreds">Объект содержащий Login и Password</param>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AuthenticateUser(LoginRequest userCreds)
        {
            var result = await authService.Login(userCreds);

            if (
                !result.IsLoggedIn
                || string.IsNullOrEmpty(result.JwtToken)
                || string.IsNullOrEmpty(result.RefreshToken)
                || result.User == null
            )
                return Unauthorized();

            AddCookie("AccessToken", result.JwtToken, DateTime.UtcNow.AddHours(4));
            AddCookie("RefreshToken", result.RefreshToken, DateTime.UtcNow.AddDays(5));

            await loggingService.AddHistoryRecordAsync(result.User, TypeActionEnum.Authorisation);

            return Ok();
        }

        // POST api/authentication/logout
        /// <summary>
        /// Метод для де авторизации пользователя
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(302)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AuthenticateUserAsync()
        {
            var model = new TokenRequest
            {
                JwtToken = Request.Cookies["AccessToken"] ?? "",
                RefreshToken = Request.Cookies["RefreshToken"] ?? ""
            };

            if (string.IsNullOrEmpty(model.JwtToken) || string.IsNullOrEmpty(model.RefreshToken)) return Unauthorized();

            await loggingService.AddHistoryRecordAsync(model.JwtToken, TypeActionEnum.LoggingOut);

            DeleteCookie("AccessToken");
            DeleteCookie("RefreshToken");

            return Ok();
        }

        // POST api/authentication/refresh-token
        /// <summary>
        /// Метод для получения нового Access токена
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken()
        {
            var model = new TokenRequest
            {
                JwtToken = Request.Cookies["AccessToken"] ?? "",
                RefreshToken = Request.Cookies["RefreshToken"] ?? ""
            };

            if (String.IsNullOrEmpty(model.JwtToken) || String.IsNullOrEmpty(model.RefreshToken)) return BadRequest("Failed to refresh token");

            var result = await authService.RefreshToken(model);

            if (
                !result.IsLoggedIn
                || string.IsNullOrEmpty(result.JwtToken)
                || string.IsNullOrEmpty(result.RefreshToken)
            )
                return BadRequest("Failed to refresh token");

            AddCookie("AccessToken", result.JwtToken, DateTime.UtcNow.AddHours(4));
            AddCookie("RefreshToken", result.RefreshToken, DateTime.UtcNow.AddDays(5));

            return Ok();
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await applicationContext.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Ok();

            // Генерация токена
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await applicationContext.SaveChangesAsync();

            // Кодирование токена для URL
            var encodedToken = Uri.EscapeDataString(token);

            // Формирование безопасной ссылки
            var resetLink = $"{configuration["WebClientAddress"]}/reset-password?token={encodedToken}";

            // Отправка письма
            await emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return Ok();
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await applicationContext.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
                return BadRequest("Недействительный токен восстановления");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await applicationContext.SaveChangesAsync();
            await loggingService.AddHistoryRecordAsync(user, TypeActionEnum.UpdateUserPassword);

            return Ok();
        }
    }
}
