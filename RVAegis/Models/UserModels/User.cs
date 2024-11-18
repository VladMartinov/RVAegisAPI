using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVAegis.Models.UserModels
{
    public class User
    {
        [Key]
        public uint UserId { get; set; }

        [ForeignKey(nameof(UserRole))]
        public required ushort UserRoleId { get; set; }
        public required UserRole UserRole { get; set; }

        [ForeignKey(nameof(UserStatus))]
        public required ushort UserStatusId { get; set; }
        public required UserStatus UserStatus { get; set; }

        [MaxLength(155)]
        public required string FullName { get; set; }
        public byte[]? Photo { get; set; }
        [MaxLength(20)]
        public required string Login { get; set; }
        public required string Password { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}