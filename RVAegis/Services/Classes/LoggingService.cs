using Microsoft.IdentityModel.Tokens;
using RVAegis.Contexts;
using RVAegis.Models.HistoryModels;
using RVAegis.Models.UserModels;
using RVAegis.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RVAegis.Services.Classes
{
    public class LoggingService(ApplicationContext applicationContext, IConfiguration configuration) : ILoggingService
    {
        private readonly ApplicationContext _applicationContext = applicationContext;
        private readonly IConfiguration _configuration = configuration;

        /* Метод по добавлению записи активности в базу данных */
        public async Task AddHistoryRecordAsync(User user, TypeActionEnum typeActionEnum)
        {
            var action = _applicationContext.TypeActions.FirstOrDefault(x => x.ActionId == (uint)typeActionEnum);

            if (action == null) return;

            var record = new HistoryRecord
            {
                DateAction = DateTime.UtcNow,
                TypeActionId = (uint)typeActionEnum,
                TypeAction = action,
                UserId = user.UserId,
                User = user,
            };

            _applicationContext.HistoryRecords.Add(record);
            await _applicationContext.SaveChangesAsync();
        }

        /* Метод по добавлению записи активности в базу данных */
        public async Task AddHistoryRecordAsync(string token, TypeActionEnum typeActionEnum)
        {
            string login = GetUserLoginFromToken(token);

            if (login == null) return;

            var user = applicationContext.Users.FirstOrDefault(u => u.Login == login);

            if (user == null) return;
            
            var action = _applicationContext.TypeActions.FirstOrDefault(x => x.ActionId == (uint)typeActionEnum);

            if (action == null) return;

            var record = new HistoryRecord
            {
                DateAction = DateTime.UtcNow,
                TypeActionId = (uint)typeActionEnum,
                TypeAction = action,
                UserId = user.UserId,
                User = user,
            };

            _applicationContext.HistoryRecords.Add(record);
            await _applicationContext.SaveChangesAsync();
        }

        /* Метод по получению логина пользователя из токена */
        private string GetUserLoginFromToken(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value ?? ""));

            var validationParameters = new TokenValidationParameters()
            {
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = securityKey
            };

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return principal?.Identity?.Name ?? string.Empty;
        }
    }
}
    