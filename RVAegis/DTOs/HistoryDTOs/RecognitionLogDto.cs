using RVAegis.Models.HistoryModels;
using System.ComponentModel.DataAnnotations;

namespace RVAegis.DTOs.HistoryDTOs
{
    public class RecognitionLogDto
    {
        public string ImageData { get; set; }
        public string Label { get; set; }
        public DateTime RecognitionTime { get; set; }
        public int CameraIndex { get; set; }

        public RecognitionLogDto(RecognitionLog recognitionLog)
        {
            ImageData = Convert.ToBase64String(recognitionLog.ImageData);
            Label = recognitionLog.Label;
            RecognitionTime = recognitionLog.RecognitionTime;
            CameraIndex = recognitionLog.CameraIndex;
        }
    }
}

