using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserRDto
    {
        public uint UserId { get; set; }

        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public string? Photo { get; set; }

        public UserRDto(User user)
        {
            UserId = user.UserId;

            UserRole = user.UserRoleId;
            UserStatus = user.UserStatusId;

            FullName = user.FullName;
            Photo = user.Photo is not null ? Convert.ToBase64String(user.Photo) : null;
        }

        public UserRDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
        }
    }
}
