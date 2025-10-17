using BarSheetAPI.Models;

namespace BarSheetAPI.DTOs
{
    public class AuthResponse
    {
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public User? User { get; set; }
    }
}