using RVAegis.Models.ImageModels;

namespace RVAegis.DTOs.ImageDTOs
{
    public class ImageCDto
    {
        public string FullName { get; set; }
        public string Photo { get; set; }

        public ImageCDto(Image image)
        {
            FullName = image.FullName;
            Photo = Convert.ToBase64String(image.Photo);
        }

        public ImageCDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
            Photo = string.Empty;
        }
    }
}
