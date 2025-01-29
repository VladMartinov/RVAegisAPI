using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.HistoryModels
{
    public enum TypeActionEnum
    {
        Authorisation = 1,
        LoggingOut = 2,
        CreateImage = 3,
        UpdateImage = 4,
        DeleteImage = 5,
        CreateUser = 6,
        UpdateUser = 7,
        ChangeUserStatus = 8,
        UpdateUserPassword = 9,
        DeleteUser = 10,
    }

    public class TypeAction
    {
        [Key]
        public uint ActionId { get; set; }

        [MaxLength(200)]
        public required string ActionDescription { get; set; }

        public required ICollection<HistoryRecord> HistoryRecords { get; set; }
    }
}
