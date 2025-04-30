using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RVAegis.Contexts;
using RVAegis.Models.HistoryModels;
using RVAegis.Models.UserModels;
using RVAegis.Services.Interfaces;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RVAegis.Services.Classes
{
    public class LoggingService(ApplicationContext applicationContext, IConfiguration configuration) : ILoggingService
    {
        // Конфигурация интервалов логирования
        private readonly TimeSpan _defaultLogInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Logging:Intervals:Default", 120));
        private readonly TimeSpan _unknownLabelLogInterval = TimeSpan.FromSeconds(configuration.GetValue<int>("Logging:Intervals:Unknown", 5));

        /* Метод по добавлению записи активности в базу данных */
        public async Task AddHistoryRecordAsync(User user, TypeActionEnum typeActionEnum)
        {
            var action = applicationContext.TypeActions.FirstOrDefault(x => x.ActionId == (uint)typeActionEnum);

            if (action == null) return;

            var record = new HistoryRecord
            {
                DateAction = DateTime.UtcNow,
                TypeActionId = (uint)typeActionEnum,
                TypeAction = action,
                UserId = user.UserId,
                User = user,
            };

            applicationContext.HistoryRecords.Add(record);
            await applicationContext.SaveChangesAsync();
        }

        /* Метод по добавлению записи активности в базу данных */
        public async Task AddHistoryRecordAsync(string token, TypeActionEnum typeActionEnum)
        {
            string login = GetUserLoginFromToken(token);

            if (login == null) return;

            var user = applicationContext.Users.FirstOrDefault(u => u.Login == login);

            if (user == null) return;
            
            var action = applicationContext.TypeActions.FirstOrDefault(x => x.ActionId == (uint)typeActionEnum);

            if (action == null) return;

            var record = new HistoryRecord
            {
                DateAction = DateTime.UtcNow,
                TypeActionId = (uint)typeActionEnum,
                TypeAction = action,
                UserId = user.UserId,
                User = user,
            };

            applicationContext.HistoryRecords.Add(record);
            await applicationContext.SaveChangesAsync();
        }

        /* Метод по получению логина пользователя из токена */
        private string GetUserLoginFromToken(string token)
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

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return principal?.Identity?.Name ?? string.Empty;
        }

        public async Task LogRecognitionAsync(string label, byte[] imageData, int cameraIndex)
        {
            var now = DateTime.UtcNow;
            var logInterval = label.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
                ? _unknownLabelLogInterval
                : _defaultLogInterval;

            // Проверка в базе данных
            var lastLoggedInDb = await applicationContext.RecognitionLogs
                .Where(log => log.Label == label)
                .OrderByDescending(log => log.RecognitionTime)
                .FirstOrDefaultAsync();

            if (lastLoggedInDb != null && (now - lastLoggedInDb.RecognitionTime) < logInterval)
            {
                return;
            }

            // Логируем событие
            var log = new RecognitionLog
            {
                RecognitionTime = now,
                Label = label,
                ImageData = imageData,
                CameraIndex = cameraIndex
            };

            applicationContext.RecognitionLogs.Add(log);
            await applicationContext.SaveChangesAsync();
        }
    }
}