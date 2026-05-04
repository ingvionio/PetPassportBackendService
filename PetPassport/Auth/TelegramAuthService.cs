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
            _botToken = config["Telegram:BotToken"]
                ?? config["BOT_TOKEN"]
                ?? Environment.GetEnvironmentVariable("BOT_TOKEN")
                ?? "";
            Console.WriteLine($"[TG] botToken loaded, length={_botToken.Length}");
        }

        /// <summary>
        /// Verifies Telegram WebApp initData HMAC signature.
        /// Returns parsed user info if valid, null otherwise.
        /// </summary>
        public TelegramUserInfo? Verify(string initData)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(initData))
            {
                Console.WriteLine($"[TG] Verify failed: botToken empty={string.IsNullOrEmpty(_botToken)}, initData empty={string.IsNullOrEmpty(initData)}");
                return null;
            }

            var pairs = initData.Split('&')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(
                    p => Uri.UnescapeDataString(p[0]),
                    p => Uri.UnescapeDataString(p[1]));

            Console.WriteLine($"[TG] keys: {string.Join(", ", pairs.Keys)}");

            if (!pairs.TryGetValue("hash", out var receivedHash))
            {
                Console.WriteLine("[TG] Verify failed: no hash field");
                return null;
            }

            // Build data_check_string: sorted key=value pairs excluding "hash" and "signature", joined with \n
            var dataCheckString = string.Join("\n", pairs
                .Where(kv => kv.Key != "hash" && kv.Key != "signature")
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key}={kv.Value}"));

            Console.WriteLine($"[TG] dataCheckString keys used: {string.Join(", ", pairs.Keys.Where(k => k != "hash" && k != "signature").OrderBy(k => k))}");

            // secret_key = HMAC_SHA256("WebAppData", bot_token)
            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes("WebAppData"),
                Encoding.UTF8.GetBytes(_botToken));

            // expected hash = HMAC_SHA256(secret_key, data_check_string)
            var expectedHash = HMACSHA256.HashData(
                secretKey,
                Encoding.UTF8.GetBytes(dataCheckString));

            var expectedHashHex = Convert.ToHexString(expectedHash).ToLower();

            Console.WriteLine($"[TG] receivedHash={receivedHash[..8]}... expectedHash={expectedHashHex[..8]}... match={string.Equals(receivedHash, expectedHashHex, StringComparison.OrdinalIgnoreCase)}");

            if (!string.Equals(receivedHash, expectedHashHex, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[TG] Verify failed: HMAC mismatch");
                return null;
            }

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
