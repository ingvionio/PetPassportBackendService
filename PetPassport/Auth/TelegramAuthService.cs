using System.Text.Json;

namespace PetPassport.Auth
{
    public class TelegramAuthService
    {
        public TelegramUserInfo? Verify(string initData)
        {
            if (string.IsNullOrEmpty(initData))
                return null;

            var pairs = initData.Split('&')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(
                    p => Uri.UnescapeDataString(p[0]),
                    p => Uri.UnescapeDataString(p[1]));

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
