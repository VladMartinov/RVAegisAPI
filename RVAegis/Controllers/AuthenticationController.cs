using Microsoft.AspNetCore.Mvc;
using RVAegis.Contexts;
using RVAegis.Models.Auth;
using RVAegis.Models.AuthModels;
using RVAegis.Models.HistoryModels;
using RVAegis.Services.Classes;
using RVAegis.Services.Interfaces;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController(ApplicationContext applicationContext, IAuthService authService, ILoggingService loggingService) : Controller
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

            HttpContext.Response.Cookies.Delete("AccessToken");
            HttpContext.Response.Cookies.Delete("RefreshToken");

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

        // PUT api/authentication/changepassword
        /// <summary>
        /// Метод по обновлению пароля пользователя
        /// </summary>
        /// <param name="userCreds">Объект содержащий Login и Password</param>
        [HttpPut("changepassword")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeUserPasswordAsync(LoginRequest userCreds)
        {
            var user = applicationContext.Users.FirstOrDefault(u => u.Login == userCreds.Login);
            if (user == null) return NotFound("User with this login not found");

            if (string.IsNullOrWhiteSpace(userCreds.Password)) return BadRequest("Failed to update password");

            user.Password = BCrypt.Net.BCrypt.HashPassword(userCreds.Password);
            await applicationContext.SaveChangesAsync();

            await loggingService.AddHistoryRecordAsync(user, TypeActionEnum.UpdateUserPassword);

            return Ok();
        }
    }
}
