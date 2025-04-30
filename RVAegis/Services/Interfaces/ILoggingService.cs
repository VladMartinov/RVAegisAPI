using RVAegis.Models.HistoryModels;
using RVAegis.Models.UserModels;

namespace RVAegis.Services.Interfaces
{
    public interface ILoggingService
    {
        Task AddHistoryRecordAsync(User user, TypeActionEnum typeActionEnum);
        Task AddHistoryRecordAsync(string token, TypeActionEnum typeActionEnum);
        Task LogRecognitionAsync(string label, byte[] imageData, int cameraIndex);
    }
}
