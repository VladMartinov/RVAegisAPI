using Microsoft.AspNetCore.Mvc;
using RVAegis.Contexts;
using RVAegis.Helpers;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/grpc")]
    public class GRPCController : Controller
    {
        private readonly ApplicationContext _applicationContext;
        private readonly FaceRecognitionClient _faceRecognitionClient;

        public GRPCController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;

            // Указываем адрес gRPC сервера
            string grpcServerAddress = "http://localhost:50052";

            // Создаём клиент
            _faceRecognitionClient = new FaceRecognitionClient(grpcServerAddress);
        }

        // POST api/grpc/load-images
        /// <summary>
        /// Метод по загрузке множества изображений
        /// </summary>
        [HttpPost("load-images")]
        public async Task<IActionResult> LoadImagesAsync()
        {
            try
            {
                var imageData = _applicationContext.Images
                    .Select(image => new
                    {
                        image.Photo,
                        image.FullName
                    })
                    .ToList();

                var images = imageData.Select(data => data.Photo).ToList();
                var labels = imageData.Select(data => data.FullName).ToList();

                // Вызываем метод gRPC клиента
                bool success = await _faceRecognitionClient.SendImagesAsync(images, labels);

                if (success)
                    return Ok("Images and labels sent successfully");
                else
                    return BadRequest("Failed to send images and labels");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing images: {ex.Message}");
            }
        }
    }
}
