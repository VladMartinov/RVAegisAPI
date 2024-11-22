using RVAegis.Contexts;
using RVAegis.Models.UserModels;
using RVAegis.DTOs.UserDTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController(ApplicationContext applicationContext) : Controller
    {
        private readonly ApplicationContext applicationContext = applicationContext;

        // GET api/users
        /// <summary>
        /// Получение всех пользователей системы.
        /// </summary>
        /// <remarks>
        /// Получение списка всех пользователей системы в формате UserRDto
        /// </remarks>
        /// <returns>Список пользователей системы</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserRDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetUsers()
        {
            var usersDtos = new List<UserRDto>();

            foreach (var user in applicationContext.Users)
                usersDtos.Add(new UserRDto(user));

            return Ok(usersDtos);
        }

        // GET api/users/current-user
        /// <summary>
        /// Получение информации об текущем пользователе.
        /// </summary>
        /// <remarks>
        /// Получение списка всех пользователей системы в формате UserRDto
        /// </remarks>
        /// <returns>Текущий пользователь</returns>
        [HttpGet("current-user")]
        [ProducesResponseType(typeof(UserRDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCurrentUser()
        {
            string userLogin = Request.HttpContext.User.Identity?.Name ?? string.Empty;

            var currentUser = applicationContext.Users.SingleOrDefault(u => u.Login == userLogin);

            if (currentUser == null) return Unauthorized();

            var currentUserReview = new UserRDto(currentUser);

            return Ok(currentUserReview);
        }

        // GET api/users/{id}
        /// <summary>
        /// Получение пользователя системы по его ID
        /// </summary>
        /// <remarks>
        /// Получение конкретного пользователя системы в формате UserRDto.
        /// </remarks>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        /// <returns>Найденный пользователь</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserRDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetUser(uint id)
        {
            var user = applicationContext.Users.SingleOrDefault(u => u.UserId == id);

            if (user == null) return NotFound("User by this ID not found");

            var userDto = new UserRDto(user);

            return Ok(userDto);
        }

        // POST api/users
        /// <summary>
        /// Создание нового пользователя системы
        /// </summary>
        /// <remarks>
        /// UserRole: 1 - Пользователь, 2 - Наблюдатель, 3 - Админ; UserStatus: 1 - Активный, 2 - Заблокированный, 3 - Удалённый.
        /// </remarks>
        /// <param name="userCDto">Пользователь в формате UserCDto</param>
        /// <returns>Новый пользователь системы</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserCDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser(UserCDto userCDto)
        {
            if (applicationContext.Users.Any(x => x.Login == userCDto.Login))
                return StatusCode(400, $"The user with login: \"${userCDto.Login}\" is already exist!");

            var role = applicationContext.UserRoles.FirstOrDefault(x => x.RoleId == userCDto.UserRole);
            var status = applicationContext.UserStatuses.FirstOrDefault(x => x.StatusId == userCDto.UserStatus);

            if (role == null || status == null)
            {
                return BadRequest("User role or status is not define.");
            }

            var user = new User
            {
                UserRoleId = userCDto.UserRole,
                UserRole = role,
                UserStatusId = userCDto.UserStatus,
                UserStatus = status,
                FullName = userCDto.FullName,
                Photo = userCDto.Photo is not null ? Convert.FromBase64String(userCDto.Photo) : null,
                Login = userCDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userCDto.Password)
            };

            applicationContext.Users.Add(user);
            await applicationContext.SaveChangesAsync();

            var createdUserDto = new UserCDto(user);
            return CreatedAtAction(nameof(CreateUser), new { id = user.UserId }, createdUserDto);
        }

        // PUT api/users/{id}
        /// <summary>
        /// Обновление пользователя по уникальному идентификатору
        /// </summary>
        /// <remarks>
        /// UserRole: 1 - Пользователь, 2 - Наблюдатель, 3 - Админ; UserStatus: 1 - Активный, 2 - Заблокированный, 3 - Удалённый.
        /// </remarks>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        /// <param name="userUDto">Пользователь в формате UserUDto</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserRDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(uint id, UserUDto userUDto)
        {
            var userToUpdate = applicationContext.Users.SingleOrDefault(u => u.UserId == id);

            if (userToUpdate == null) return NotFound("User by this ID not founded");

            var role = applicationContext.UserRoles.FirstOrDefault(x => x.RoleId == userUDto.UserRole);
            var status = applicationContext.UserStatuses.FirstOrDefault(x => x.StatusId == userUDto.UserStatus);

            if (role == null || status == null)
            {
                return BadRequest("User role or status is not define.");
            }

            userToUpdate.UserRoleId = userUDto.UserRole;
            userToUpdate.UserRole = role;

            userToUpdate.UserStatusId = userUDto.UserStatus;
            userToUpdate.UserStatus = status;

            userToUpdate.FullName = userUDto.FullName;
            if (!string.IsNullOrEmpty(userUDto.Photo)) userToUpdate.Photo = Convert.FromBase64String(userUDto.Photo);

            await applicationContext.SaveChangesAsync();

            var updatedUserDto = new UserRDto(userToUpdate);
            return Ok(updatedUserDto);
        }

        // DELETE api/users/{id}
        /// <summary>
        /// Удаление пользователя в системе
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого пользователя</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(uint id)
        {
            var user = applicationContext.Users.SingleOrDefault(u => u.UserId == id);

            if (user == null) return NotFound("User by this ID not founded");

            var status = applicationContext.UserStatuses.FirstOrDefault(x => x.StatusId == (ushort)UserStatusEnum.Removed);

            if (status == null)
            {
                return BadRequest("User status is not define.");
            }

            user.UserStatusId = (ushort)UserStatusEnum.Removed;
            user.UserStatus = status;

            await applicationContext.SaveChangesAsync();

            return Ok();
        }

        // PUT api/users/{id}/status
        /// <summary>
        /// Обновление статуса пользователя
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого пользователя</param>
        /// <param name="status">Код статуса</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(UserRDto), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUserStatu(uint id, ushort status)
        {
            var user = applicationContext.Users.SingleOrDefault(u => u.UserId == id);

            if (user == null) return NotFound("User by this ID not founded");

            var statusObj = applicationContext.UserStatuses.FirstOrDefault(x => x.StatusId == status);

            if (statusObj == null)
            {
                return BadRequest("User status is not define.");
            }

            user.UserStatusId = status;
            user.UserStatus = statusObj;

            await applicationContext.SaveChangesAsync();

            var userDto = new UserRDto(user);
            return Ok(userDto);
        }
    }
}
