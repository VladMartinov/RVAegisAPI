using RVAegis.Models.UserModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.HistoryModels
{
    public class HistoryRecord
    {
        [Key]
        public uint HistoryRecordId { get; set; }

        public DateTime DateAction { get; set; }

        [ForeignKey(nameof(TypeAction))]
        public uint TypeActionId { get; set; }
        public required TypeAction TypeAction { get; set; }

        [ForeignKey(nameof(User))]
        public uint UserId { get; set; }
        public User User { get; set; }
    }
}