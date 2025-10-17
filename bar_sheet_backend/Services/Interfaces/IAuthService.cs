using BarSheetAPI.DTOs;

namespace BarSheetAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(UserRegisterDto dto);
        Task<AuthResponse> LoginAsync(UserLoginDto dto);
        Task<AuthResponse> RefreshTokenAsync(TokenRequestDto dto);
    }
}
