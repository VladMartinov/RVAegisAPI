using Microsoft.AspNetCore.Mvc;
using RVAegis.Contexts;
using RVAegis.DTOs.ImageDTOs;
using RVAegis.Models.HistoryModels;
using RVAegis.Models.ImageModels;
using RVAegis.Services.Interfaces;

namespace RVAegis.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/images")]
    public class ImagesController(ApplicationContext applicationContext, ILoggingService loggingService) : Controller
    {
        // GET api/images
        /// <summary>
        /// Получение всех изображений.
        /// </summary>
        /// <returns>Список изображений</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ImageRUDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetImages()
        {
            var imagesDtos = new List<ImageRUDto>();

            foreach (var image in applicationContext.Images)
                imagesDtos.Add(new ImageRUDto(image));

            return Ok(imagesDtos);
        }

        // GET api/images/{id}
        /// <summary>
        /// Получение изображения по его ID
        /// </summary>
        /// <param name="id">Уникальный идентификатор изображения</param>
        /// <returns>Найденное изображение</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ImageRUDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetImage(uint id)
        {
            var image = applicationContext.Images.SingleOrDefault(u => u.ImageId == id);

            if (image == null) return NotFound("Image by this ID not found");

            return Ok(new ImageRUDto(image));
        }

        // POST api/images
        /// <summary>
        /// Создание нового изображения
        /// </summary>
        /// <param name="image">Объект изображение с самим файлом и полным именем</param>
        /// <returns>Новое изображение</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ImageRUDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateImage(ImageCDto image)
        {
            var newImage = new Image
            {
                FullName = image.FullName,
                Photo = Convert.FromBase64String(image.Photo),
                DateCreate = DateTime.UtcNow,
            };

            applicationContext.Images.Add(newImage);

            await applicationContext.SaveChangesAsync();

            await loggingService.AddHistoryRecordAsync(Request.Cookies["AccessToken"] ?? string.Empty, TypeActionEnum.CreateImage);

            var createdImageDto = new ImageRUDto(newImage);
            return CreatedAtAction(nameof(CreateImage), new { id = newImage.ImageId }, createdImageDto);
        }

        // PUT api/images/{id}
        /// <summary>
        /// Обновление изображения по уникальному идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор изображения</param>
        /// <param name="image">Изображение в формате ImageRUDto</param>
        /// <returns>Обновленное изображение</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ImageRUDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateImage(uint id, ImageRUDto image)
        {
            var imageToUpdate = applicationContext.Images.SingleOrDefault(u => u.ImageId == id);

            if (imageToUpdate == null) return NotFound("Image by this ID not founded");

            imageToUpdate.FullName = image.FullName;
            imageToUpdate.Photo = Convert.FromBase64String(image.Photo);

            imageToUpdate.DateUpdate = DateTime.UtcNow;

            await applicationContext.SaveChangesAsync();

            await loggingService.AddHistoryRecordAsync(Request.Cookies["AccessToken"] ?? string.Empty, TypeActionEnum.UpdateImage);

            return Ok(new ImageRUDto(imageToUpdate));
        }

        // DELETE api/images/{id}
        /// <summary>
        /// Удаление изображение из системы
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого изображения</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteImage(uint id)
        {
            var image = applicationContext.Images.SingleOrDefault(u => u.ImageId == id);

            if (image == null) return NotFound("Image by this ID not founded");

            applicationContext.Images.Remove(image);

            await applicationContext.SaveChangesAsync();

            await loggingService.AddHistoryRecordAsync(Request.Cookies["AccessToken"] ?? string.Empty, TypeActionEnum.DeleteImage);

            return Ok();
        }
    }
}
