namespace RVAegis.Models.AuthModels
{
    public class TokenRequest
    {
        public required string JwtToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
