namespace PetPassport.Auth
{
    public class TelegramAuthRequest
    {
        /// <summary>Raw initData string from window.Telegram.WebApp.initData</summary>
        public string InitData { get; set; } = null!;
    }

    public class LoginRequest
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public int OwnerId { get; set; }
    }
}
