using Google.Protobuf.Collections;
using Google.Protobuf;
using Grpc.Core;
using RVAegis.Contexts;

namespace RVAegis.Services.Classes
{
    public class ImageLoaderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly FaceRecognition.FaceRecognitionClient _grpcClient;
        private readonly TimeSpan _interval;

        public ImageLoaderService(
            IServiceProvider serviceProvider,
            FaceRecognition.FaceRecognitionClient grpcClient,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _grpcClient = grpcClient;

            // Парсим интервал из конфига или используем значение по умолчанию (240 минут)
            if (!int.TryParse(configuration["gRPC:ImageLoadInterval"], out int intervalMinutes))
            {
                intervalMinutes = 240;
            }

            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Загружаем изображения сразу при старте
            await LoadImagesAsync(stoppingToken);

            // Периодически загружаем изображения
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                await LoadImagesAsync(stoppingToken);
            }
        }

        private async Task LoadImagesAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                try
                {
                    var imageData = dbContext.Images
                        .Select(image => new
                        {
                            image.Photo,
                            image.FullName
                        })
                        .ToList();

                    var images = imageData.Select(data => data.Photo).ToList();
                    var labels = imageData.Select(data => data.FullName).ToList();

                    var imageByteStrings = new RepeatedField<ByteString>();
                    foreach (var image in images)
                        imageByteStrings.Add(ByteString.CopyFrom(image));

                    var request = new ImageRequest { Images = { imageByteStrings }, Labels = { labels } };
                    var response = await _grpcClient.SendImagesAsync(request, cancellationToken: stoppingToken);

                    if (response.Success)
                    {
                        Console.WriteLine("Images and labels sent successfully");
                    }
                    else
                    {
                        Console.WriteLine("Failed to send images and labels");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    Console.WriteLine($"Ошибка подключения к gRPC-серверу: {ex.Status}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке изображений: {ex.Message}");
                }
            }
        }
    }
}