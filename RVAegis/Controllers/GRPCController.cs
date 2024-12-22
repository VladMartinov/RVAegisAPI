using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using RVAegis.Contexts;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/grpc")]
    public class GRPCController(ApplicationContext applicationContext) : Controller
    {
        // POST api/grpc/process-image
        /// <summary>
        /// Метод по обработки изображения
        /// </summary>
        [HttpPost("process-image")]
        public async Task<IActionResult> ProcessImageAsync([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                ms.Position = 0;

                using var channel = GrpcChannel.ForAddress("http://localhost:5096");
                var client = new ImageTransfer.ImageTransferClient(channel);

                var request = new ImageRequest { Image = Google.Protobuf.ByteString.CopyFrom(ms.ToArray()) };
                var reply = await client.TransferImageAsync(request);

                return File(reply.Image.ToByteArray(), "image/jpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing image: {ex.Message}");
            }
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
                var images = applicationContext.Images.Select(image => ByteString.CopyFrom(image.Photo)).ToList();
      
                using var channel = GrpcChannel.ForAddress("http://localhost:5096");
                var client = new ImageTransfer.ImageTransferClient(channel);
      
                var request = new MultipleImageRequest { Images = { images } };
                var reply = await client.TransferMultipleImagesAsync(request);
                return Ok(reply.Count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing images: {ex.Message}");
            }
        }
    }
}
