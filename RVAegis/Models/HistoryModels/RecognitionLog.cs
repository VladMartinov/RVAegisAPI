using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.HistoryModels
{
    public class RecognitionLog
    {
        [Key]
        public uint RecognitionLogId { get; set; }

        [Required]
        public byte[] ImageData { get; set; } = [];

        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        public DateTime RecognitionTime { get; set; }
        public int CameraIndex { get; set; }
    }
}
