using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.ImageModels
{
    public class Image
    {
        [Key]
        public uint ImageId { get; set; }

        [MaxLength(155)]
        public required string FullName { get; set; }
        public required byte[] Photo { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
