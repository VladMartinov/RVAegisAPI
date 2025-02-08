using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserRoleDto
    {
        public ushort RoleId { get; set; }
        public string RoleTitle { get; set; }

        public UserRoleDto(UserRole role)
        {
            RoleId = role.RoleId;
            RoleTitle = role.RoleTitle;
        }

        public UserRoleDto()
        {
            // Empty constructor for deserialization
            RoleTitle = string.Empty;
        }
    }
}
