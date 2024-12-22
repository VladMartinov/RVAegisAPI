using Grpc.Core;
using Google.Protobuf;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RVAegisGrpcService.Services
{
    public class ImageTransferService(ILogger<ImageTransferService> logger, IHttpClientFactory httpClientFactory) : ImageTransfer.ImageTransferBase
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

        public override async Task<ImageResponse> TransferImage(ImageRequest request, ServerCallContext context)
        {
            try
            {
                var imageContent = new ByteArrayContent(request.Image.ToByteArray());
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                var content = new MultipartFormDataContent("multipart/form-data")
                {
                    { imageContent, "image", "image.jpg" }
                };

                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/process_image", content);

                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var ms2 = new MemoryStream();
                await responseStream.CopyToAsync(ms2);
                ms2.Position = 0;

                var imageData = ms2.ToArray();
                return new ImageResponse { Image = ByteString.CopyFrom(imageData) };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при обработке изображения");
                return new ImageResponse { Image = ByteString.Empty };
            }
        }

        public override async Task<MultipleImageResponse> TransferMultipleImages(MultipleImageRequest request, ServerCallContext context)
        {
            try
            {
                if (request.Images.Count == 0) throw new ArgumentException("No images provided in the request.");

                var content = new MultipartFormDataContent("multipart/form-data");
                for (int i = 0; i < request.Images.Count; i++)
                {
                    var imageBytes = request.Images[i];
                    var imageContent = new ByteArrayContent(imageBytes.ToByteArray());
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(imageContent, "images", $"image_{i}.jpg");
                }

                var response = await _httpClient.PostAsync("http://127.0.0.1:8000/load_images", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                var loadResponse = JsonSerializer.Deserialize<MultipleImageResponseModel>(responseString);
                if (loadResponse != null)
                {
                    return new MultipleImageResponse { Status = loadResponse.status, Count = loadResponse.count };
                }

                var errorResponse = JsonSerializer.Deserialize<MultipleImageErrorResponseModel>(responseString);
                if (errorResponse != null)
                {
                    return new MultipleImageResponse { Status = errorResponse.status, Count = 0 };
                }

                return new MultipleImageResponse { Status = "error", Count = 0 };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return new MultipleImageResponse { Status = "error", Count = 0 };
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
                return new MultipleImageResponse { Status = "error", Count = 0 };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new MultipleImageResponse { Status = "error", Count = 0 };
            }
        }
    }
}