using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using RVAegis.Services.Interfaces;
using System.Text.Json;

namespace RVAegis.Services.Classes
{
    public class ImageBroadcastService(FaceRecognition.FaceRecognitionClient grpcClient, IServiceScopeFactory serviceScopeFactory) : BackgroundService
    {
        private readonly FaceRecognition.FaceRecognitionClient _grpcClient = grpcClient;
        private bool _isGrpcConnected = false;
        private HashSet<int> _activeCameras = [];
        private bool _isFirstStatusCheck = true;
        private readonly object _syncRoot = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await StartCameraFramesSenderAsync(stoppingToken);

            // Если понадобится функционал запроса активных камер
            // var framesTask = StartCameraFramesSenderAsync(stoppingToken);
            // var statusTask = StartCameraStatusCheckerAsync(stoppingToken);

            // await Task.WhenAll(statusTask, framesTask);
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

                var currentCameras = new HashSet<int>(result.Response.Select(r => r.CameraIndex).ToList());
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

                    bool isEmply = false;

                    foreach (var item in result.Response)
                    {
                        int cameraIndex = item.CameraIndex;

                        foreach (var frame in item.Frames)
                        {
                            if (frame.Frame != null)
                            {
                                for (int i = 0; i < frame.RecognizedLabels.Count; i++)
                                {
                                    using var scope = serviceScopeFactory.CreateScope();
                                    var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
                                    await loggingService.LogRecognitionAsync(frame.RecognizedLabels[i], frame.CroppedFaces[i].ToByteArray(), cameraIndex);
                                }
                            }

                            // Отправка через WebSocket
                            var message = new
                            {
                                type = "frames",
                                cameras = result.Response.Select(r => r.CameraIndex).ToList(),
                                cameraIndex = cameraIndex,
                                images = Convert.ToBase64String(frame.Frame.ToByteArray()),
                            };

                            string jsonMessage = JsonSerializer.Serialize(message);
                            await Helpers.WebSocketMiddleware.BroadcastJsonAsync(jsonMessage);
                        }
                    }

                    if (isEmply)
                    {
                        await Task.Delay(100, stoppingToken);
                        continue;
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

                await Task.Delay(50, stoppingToken);
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