using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserStatusDto
    {
        public ushort StatusId { get; set; }
        public string StatusTitle { get; set; }

        public UserStatusDto(UserStatus status)
        {
            StatusId = status.StatusId;
            StatusTitle = status.StatusTitle;
        }

        public UserStatusDto()
        {
            // Empty constructor for deserialization
            StatusTitle = string.Empty;
        }
    }
}
