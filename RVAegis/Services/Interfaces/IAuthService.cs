using RVAegis.Models.Auth;
using RVAegis.Models.AuthModels;

namespace RVAegis.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest user);
        Task<LoginResponse> RefreshToken(TokenRequest model);
    }
}
