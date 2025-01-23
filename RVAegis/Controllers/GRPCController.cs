using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using RVAegis.Contexts;

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
            string grpcServerAddress = "http://localhost:5096";

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

        // GET api/grpc/process-image
        /// <summary>
        /// Метод по проучению обработонного изображения
        /// </summary>
        [HttpGet("get-results")]
        public async Task<IActionResult> GetResults()
        {
            // Вызываем метод gRPC клиента
            var results = await _faceRecognitionClient.GetResultsAsync();

            return Ok(results);
        }
    }
}
