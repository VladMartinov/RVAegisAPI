using RVAegis.Models.HistoryModels;

namespace RVAegis.DTOs.HistoryDTOs
{
    public class TypeActionDto
    {
        public uint ActionId { get; set; }
        public string ActionDescription { get; set; }

        public TypeActionDto(TypeAction typeAction)
        {
            ActionId = typeAction.ActionId;
            ActionDescription = typeAction.ActionDescription;
        }

        public TypeActionDto()
        {
            // Empty constructor for deserialization
            ActionDescription = string.Empty;
        }
    }
}
