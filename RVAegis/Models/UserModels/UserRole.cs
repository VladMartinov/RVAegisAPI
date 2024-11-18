using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.UserModels
{
    public enum UserRoleEnum
    {
        User = 1,
        Observer = 2,
        Admin = 3
    }

    public class UserRole
    {
        [Key]
        public ushort RoleId { get; set; }

        [Required]
        [MaxLength(20)]
        public required string RoleTitle { get; set; }
    }
}
