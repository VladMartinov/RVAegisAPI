using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserCDto
    {
        public ushort UserRoleId { get; set; }
        public ushort UserStatusId { get; set; }

        public string FullName { get; set; }
        public string? Photo { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public UserCDto(User user)
        {
            UserRoleId = user.UserRoleId;
            UserStatusId = user.UserStatusId;

            FullName = user.FullName;
            Photo = user.Photo is not null ? Convert.ToBase64String(user.Photo) : null;

            Login = user.Login;
            Password = user.Password;
            Email = user.Email;
        }

        public UserCDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
            Login = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
        }
    }
}
