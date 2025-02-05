using RVAegis.Models.HistoryModels;

namespace RVAegis.DTOs.HistoryDTOs
{
    public class HistoryRecordDto
    {
        public DateTime DateAction { get; set; }

        public uint TypeActionId { get; set; }
        public uint UserId { get; set; }

        public HistoryRecordDto(HistoryRecord historyRecord)
        {
            DateAction = historyRecord.DateAction;

            TypeActionId = historyRecord.TypeAction.ActionId;
            UserId = historyRecord.User.UserId;
        }
    }
}
