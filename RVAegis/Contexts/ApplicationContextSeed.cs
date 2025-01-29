using RVAegis.Models.HistoryModels;
using RVAegis.Models.UserModels;

namespace RVAegis.Contexts
{
    public static class ApplicationContextSeed
    {
        public static async Task SeedAsync(ApplicationContext context)
        {
            if (!context.UserRoles.Any())
            {
                await context.UserRoles.AddRangeAsync(
                    new UserRole { RoleTitle = "Пользователь" },
                    new UserRole { RoleTitle = "Наблюдатель" },
                    new UserRole { RoleTitle = "Администратор" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.UserStatuses.Any())
            {
                await context.UserStatuses.AddRangeAsync(
                    new UserStatus { StatusTitle = "Активен" },
                    new UserStatus { StatusTitle = "Заблокирован" },
                    new UserStatus { StatusTitle = "Удалён" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                var adminRole = context.UserRoles.FirstOrDefault(r => r.RoleTitle == "Администратор");
                var activeStatus = context.UserStatuses.FirstOrDefault(s => s.StatusTitle == "Активен");

                if (adminRole != null && activeStatus != null)
                {
                    await context.Users.AddAsync(new User
                    {
                        UserRoleId = adminRole.RoleId,
                        UserRole = adminRole,
                        UserStatusId = activeStatus.StatusId,
                        UserStatus = activeStatus,
                        FullName = "Admin",
                        Photo = null,
                        Login = "RVTech\\admin",
                        Password = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
                        HistoryRecords = []
                    });
                    await context.SaveChangesAsync();
                }
                else
                {
                    //  Обработка ошибки
                    throw new Exception("Не удалось найти роль Администратор или статус Активен");
                }
            }

            if (!context.TypeActions.Any())
            {
                await context.TypeActions.AddRangeAsync(
                    new TypeAction { ActionDescription = "Авторизация в систему", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Выход из системы", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Создание изображения", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Редактирование изображения", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Удаление изображения", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Создание пользователя", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Редактирование пользователя", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Смена статуса пользователю", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Обновление пароля", HistoryRecords = [] },
                    new TypeAction { ActionDescription = "Удаление пользователя", HistoryRecords = [] }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
