using RVAegis.Models.ImageModels;

namespace RVAegis.DTOs.ImageDTOs
{
    public class ImageRUDto
    {
        public uint ImageId { get; set; }

        public string FullName { get; set; }
        public string Photo { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }

        public ImageRUDto(Image image)
        {
            ImageId = image.ImageId;
            FullName = image.FullName;
            Photo = Convert.ToBase64String(image.Photo);
            DateCreate = image.DateCreate;
            DateUpdate = image.DateUpdate;
        }

        public ImageRUDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
            Photo = string.Empty;
        }
    }
}
