using Grpc.Core;
using System.Text.Json;

namespace RVAegis.Services.Classes
{
    public class ImageBroadcastService(FaceRecognition.FaceRecognitionClient grpcClient) : BackgroundService
    {
        private readonly FaceRecognition.FaceRecognitionClient _grpcClient = grpcClient;
        private bool _isGrpcConnected = false;
        private HashSet<int> _activeCameras = new();
        private bool _isFirstStatusCheck = true;
        private readonly object _syncRoot = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var statusTask = StartCameraStatusCheckerAsync(stoppingToken);
            var framesTask = StartCameraFramesSenderAsync(stoppingToken);

            await Task.WhenAll(statusTask, framesTask);
        }

        private async Task StartCameraStatusCheckerAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
            await CheckCameraStatusAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckCameraStatusAsync(stoppingToken);
            }
        }

        private async Task CheckCameraStatusAsync(CancellationToken stoppingToken)
        {
            try
            {
                bool isConnected;
                lock (_syncRoot) isConnected = _isGrpcConnected;

                if (!isConnected && !(await CheckGrpcConnectionAsync()))
                {
                    Console.WriteLine("[INFO] Нет подключения к gRPC. Статус камер не обновлен.");
                    return;
                }

                var result = await _grpcClient.GetResultsAsync(
                    new ResultRequest(),
                    cancellationToken: stoppingToken
                );

                var currentCameras = new HashSet<int>(result.CameraFrames.Select(cf => cf.CameraIndex));
                bool hasChanged;

                lock (_syncRoot)
                {
                    hasChanged = _isFirstStatusCheck || !currentCameras.SetEquals(_activeCameras);
                    if (hasChanged)
                    {
                        _activeCameras = currentCameras;
                        _isFirstStatusCheck = false;
                    }
                }

                if (hasChanged && Helpers.WebSocketMiddleware.HasActiveConnections())
                {
                    var message = new
                    {
                        type = "status",
                        cameras = currentCameras.ToList()
                    };

                    string jsonMessage = JsonSerializer.Serialize(message);
                    Console.WriteLine($"[INFO] Обновление статуса камер: {string.Join(", ", currentCameras)}");
                    await Helpers.WebSocketMiddleware.BroadcastJsonAsync(jsonMessage);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                        ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                lock (_syncRoot) _isGrpcConnected = false;
                Console.WriteLine($"[ERROR] Ошибка gRPC при проверке статуса: {ex.Status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка проверки статуса: {ex.Message}");
                lock (_syncRoot) _isGrpcConnected = false;
            }
        }

        private async Task StartCameraFramesSenderAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!Helpers.WebSocketMiddleware.HasActiveConnections())
                    {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    bool isConnected;
                    lock (_syncRoot) isConnected = _isGrpcConnected;

                    if (!isConnected && !(await CheckGrpcConnectionAsync()))
                    {
                        await Task.Delay(5000, stoppingToken);
                        continue;
                    }

                    var result = await _grpcClient.GetResultsAsync(
                        new ResultRequest(),
                        cancellationToken: stoppingToken
                    );

                    if (result.CameraFrames.Count == 0)
                    {
                        await Task.Delay(100, stoppingToken);
                        continue;
                    }

                    foreach (var cameraFrame in result.CameraFrames)
                    {
                        var message = new
                        {
                            type = "frames",
                            cameraIndex = cameraFrame.CameraIndex,
                            images = cameraFrame.Frames.Select(f => Convert.ToBase64String(f.ToByteArray())).ToList()
                        };

                        string jsonMessage = JsonSerializer.Serialize(message);
                        await Helpers.WebSocketMiddleware.BroadcastJsonAsync(jsonMessage);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable ||
                                             ex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    lock (_syncRoot) _isGrpcConnected = false;
                    Console.WriteLine($"[ERROR] Ошибка gRPC: {ex.Status}");
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка отправки кадров: {ex.Message}");
                    lock (_syncRoot) _isGrpcConnected = false;
                    await Task.Delay(5000, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }
        }

        private async Task<bool> CheckGrpcConnectionAsync()
        {
            try
            {
                await _grpcClient.GetResultsAsync(
                    new ResultRequest(),
                    deadline: DateTime.UtcNow.AddSeconds(5)
                );
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}