using System.ComponentModel.DataAnnotations;

namespace RVAegis.Models.AuthModels
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
