using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserCDto
    {
        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public string? Photo { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }

        public UserCDto(User user)
        {
            UserRole = user.UserRoleId;
            UserStatus = user.UserStatusId;

            FullName = user.FullName;
            Photo = user.Photo is not null ? Convert.ToBase64String(user.Photo) : null;

            Login = user.Login;
            Password = user.Password;
        }

        public UserCDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
            Login = string.Empty;
            Password = string.Empty;
        }
    }
}
