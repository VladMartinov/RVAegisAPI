using Grpc.Core;
using System.Text.Json;

namespace RVAegis.Services.Classes
{
    public class ImageBroadcastService(FaceRecognition.FaceRecognitionClient grpcClient) : BackgroundService
    {
        private readonly FaceRecognition.FaceRecognitionClient _grpcClient = grpcClient;
        private bool _isGrpcConnected = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (Helpers.WebSocketMiddleware.HasActiveConnections())
                    {
                        // Проверка подключения к gRPC
                        if (!_isGrpcConnected)
                        {
                            _isGrpcConnected = await CheckGrpcConnectionAsync();
                            if (!_isGrpcConnected)
                            {
                                Console.WriteLine("Нет подключения к gRPC-серверу. Повторная попытка через 5 секунд...");
                                await Task.Delay(5000, stoppingToken);
                                continue;
                            }
                        }

                        // Запрос результатов
                        var result = await _grpcClient.GetResultsAsync(new ResultRequest(), cancellationToken: stoppingToken);
                        Console.WriteLine($"Получение результата. Количество камер: {result.CameraFrames.Count}");

                        if (result.CameraFrames.Count > 0)
                        {
                            foreach (var cameraFrame in result.CameraFrames)
                            {
                                var message = new
                                {
                                    cameraIndex = cameraFrame.CameraIndex,
                                    images = cameraFrame.Frames.Select(f => Convert.ToBase64String(f.ToByteArray())).ToList()
                                };

                                string jsonMessage = JsonSerializer.Serialize(message);
                                Console.WriteLine($"Отправка данных для камеры {cameraFrame.CameraIndex} (количество изображений: {cameraFrame.Frames.Count})");
                                await Helpers.WebSocketMiddleware.BroadcastJsonAsync(jsonMessage);
                            }
                        }
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    // Обработка ошибок подключения
                    _isGrpcConnected = false;
                    Console.WriteLine($"Ошибка подключения к gRPC-серверу: {ex.Status}. Повторная попытка через 5 секунд...");
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    // Обработка других ошибок
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    _isGrpcConnected = false;
                    await Task.Delay(5000, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }
        }

        // Метод для проверки подключения к gRPC
        private async Task<bool> CheckGrpcConnectionAsync()
        {
            try
            {
                await _grpcClient.GetResultsAsync(new ResultRequest(), deadline: DateTime.UtcNow.AddSeconds(5));
                return true;
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine($"Ошибка проверки подключения к gRPC-серверу: {ex.Status}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке подключения: {ex.Message}");
                return false;
            }
        }
    }
}