namespace RVAegis.Services.Classes
{
    public class ImageBroadcastService(FaceRecognition.FaceRecognitionClient grpcClient) : BackgroundService
    {
        private readonly FaceRecognition.FaceRecognitionClient _grpcClient = grpcClient;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Helpers.WebSocketMiddleware.HasActiveConnections())
                {
                    var result = await _grpcClient.GetResultsAsync(new ResultRequest());

                    Console.WriteLine($"Geeting result. Image count: {result.ProcessedImages.Count}");
                    if (result.ProcessedImages.Count != 0)
                    {
                        foreach (var imageData in result.ProcessedImages)
                        {
                            Console.WriteLine($"Sending image data of size: {imageData.Length} bytes");
                            await Helpers.WebSocketMiddleware.BroadcastImageAsync(imageData.ToByteArray());
                        }
                    }
                }

                await Task.Delay(100, stoppingToken); // Задержка между запросами
            }
        }
    }
}