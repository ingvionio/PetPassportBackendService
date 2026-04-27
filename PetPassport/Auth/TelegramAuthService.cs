using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PetPassport.Auth
{
    public class TelegramAuthService
    {
        private readonly string _botToken;

        public TelegramAuthService(IConfiguration config)
        {
            // Читаем из Telegram:BotToken (appsettings) или BOT_TOKEN (env var / docker-compose)
            _botToken = config["Telegram:BotToken"]
                ?? config["BOT_TOKEN"]
                ?? "";
        }

        /// <summary>
        /// Verifies Telegram WebApp initData HMAC signature.
        /// Returns parsed user info if valid, null otherwise.
        /// </summary>
        public TelegramUserInfo? Verify(string initData)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(initData))
                return null;

            var pairs = initData.Split('&')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(
                    p => Uri.UnescapeDataString(p[0]),
                    p => Uri.UnescapeDataString(p[1]));

            if (!pairs.TryGetValue("hash", out var receivedHash))
                return null;

            // Build data_check_string: sorted key=value pairs excluding "hash", joined with \n
            var dataCheckString = string.Join("\n", pairs
                .Where(kv => kv.Key != "hash")
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}"));

            // secret_key = HMAC_SHA256("WebAppData", bot_token)
            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes("WebAppData"),
                Encoding.UTF8.GetBytes(_botToken));

            // expected hash = HMAC_SHA256(secret_key, data_check_string)
            var expectedHash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(dataCheckString));

            var expectedHashHex = Convert.ToHexString(expectedHash).ToLower();

            if (!string.Equals(receivedHash, expectedHashHex, StringComparison.OrdinalIgnoreCase))
                return null;

            // Optional: reject data older than 24 hours
            if (pairs.TryGetValue("auth_date", out var authDateStr)
                && long.TryParse(authDateStr, out var authDateUnix))
            {
                var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix).UtcDateTime;
                if (DateTime.UtcNow - authDate > TimeSpan.FromHours(24))
                    return null;
            }

            if (!pairs.TryGetValue("user", out var userJson))
                return null;

            try
            {
                var user = JsonSerializer.Deserialize<TelegramUserJson>(userJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (user is null || user.Id == 0)
                    return null;

                return new TelegramUserInfo
                {
                    TelegramId = user.Id,
                    Username = user.Username
                };
            }
            catch
            {
                return null;
            }
        }

        private class TelegramUserJson
        {
            public long Id { get; set; }
            public string? Username { get; set; }
            public string? First_name { get; set; }
        }
    }

    public class TelegramUserInfo
    {
        public long TelegramId { get; set; }
        public string? Username { get; set; }
    }
}
