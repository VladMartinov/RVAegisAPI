using Google.Protobuf.Collections;
using Google.Protobuf;
using Grpc.Core;

namespace RVAegisGrpcService
{
    public class FaceRecognitionService : FaceRecognition.FaceRecognitionBase
    {
        public override Task<ImageResponse> SendImages(ImageRequest request, ServerCallContext context)
        {
            // Получаем изображения и метки
            var images = request.Images;
            var labels = request.Labels;

            // Логируем получение запроса
            Console.WriteLine("[INFO] Received request to send images and labels.");
            // Логируем количество полученных изображений и лейблов
            Console.WriteLine($"[INFO] Received {request.Images.Count} images and {request.Labels.Count} labels.");

            // Здесь можно добавить логику для передачи данных в Python ИИ
            // Например, сохранить изображения и метки в базу данных или отправить их через другой gRPC канал

            // Логируем успешную обработку
            Console.WriteLine("[INFO] Images and labels processed successfully.");

            return Task.FromResult(new ImageResponse
            {
                Success = true,
                Message = "Images and labels received successfully"
            });
        }

        public override Task<ResultResponse> GetResults(ResultRequest request, ServerCallContext context)
        {
            // Логируем получение запроса
            Console.WriteLine("[INFO] Received request to get results.");

            // Здесь можно добавить логику для получения результатов от Python ИИ
            // Например, получить обработанные изображения и метки

            var processedImages = new RepeatedField<ByteString>();
            var recognizedLabels = new RepeatedField<string>();

            // Логируем возврат результатов
            Console.WriteLine("[INFO] Returning results.");

            return Task.FromResult(new ResultResponse
            {
                ProcessedImages = { processedImages },
                RecognizedLabels = { recognizedLabels }
            });
        }
    }
}
