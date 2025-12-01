using System.Net.Http.Json;

namespace PetPassport.Services
{
    public class BotNotificationService : IBotNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BotNotificationService> _logger;
        private readonly string _botServiceUrl;

        public BotNotificationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<BotNotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _botServiceUrl = configuration["BotService:BaseUrl"] 
                ?? throw new InvalidOperationException(
                    "BotService:BaseUrl не настроен. Добавьте в appsettings.json или appsettings.Development.json: " +
                    "\"BotService\": { \"BaseUrl\": \"http://your-bot-service-url:port\" }");
        }

        public async Task<bool> SendReminderAsync(long telegramId, string petName, string eventType, string eventTitle, DateTime eventDate)
        {
            var url = $"{_botServiceUrl.TrimEnd('/')}/message";
            
            try
            {
                var reminderData = new ReminderNotificationDto
                {
                    TelegramId = telegramId,
                    PetName = petName,
                    EventType = eventType,
                    EventTitle = eventTitle,
                    EventDate = eventDate
                };

                // Логируем начало отправки с полными данными
                _logger.LogInformation(
                    "═══════════════════════════════════════════════════════════");
                _logger.LogInformation(
                    "📤 ОТПРАВКА НАПОМИНАНИЯ В СЕРВИС БОТА");
                _logger.LogInformation(
                    "   URL: {Url}", url);
                _logger.LogInformation(
                    "   Данные: TelegramId={TelegramId}, PetName={PetName}, EventType={EventType}, EventTitle={EventTitle}, EventDate={EventDate}",
                    telegramId, petName, eventType, eventTitle, eventDate);
                _logger.LogInformation(
                    "═══════════════════════════════════════════════════════════");

                var startTime = DateTime.UtcNow;
                var response = await _httpClient.PostAsJsonAsync(url, reminderData);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                // Логируем ответ
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation(
                    "═══════════════════════════════════════════════════════════");
                _logger.LogInformation(
                    "📥 ОТВЕТ ОТ СЕРВИСА БОТА");
                _logger.LogInformation(
                    "   StatusCode: {StatusCode} ({StatusCodeNumber})", 
                    response.StatusCode, (int)response.StatusCode);
                _logger.LogInformation(
                    "   Время ответа: {Duration} мс", duration);
                _logger.LogInformation(
                    "   Тело ответа: {ResponseContent}", responseContent);
                _logger.LogInformation(
                    "═══════════════════════════════════════════════════════════");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "✅ УСПЕХ: Напоминание успешно отправлено в сервис бота! TelegramId={TelegramId}, EventType={EventType}",
                        telegramId, eventType);
                    return true;
                }
                else
                {
                    _logger.LogWarning(
                        "❌ ОШИБКА: Сервис бота вернул ошибку. TelegramId={TelegramId}, StatusCode={StatusCode}, Response={Response}",
                        telegramId, response.StatusCode, responseContent);
                    return false;
                }
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                _logger.LogError(
                    "❌ ОШИБКА СЕТИ: Не удалось отправить запрос на {Url}. TelegramId={TelegramId}, Error={Error}",
                    url, telegramId, httpEx.Message);
                _logger.LogError("   Проверьте, что сервис бота запущен и доступен по адресу: {Url}", url);
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(
                    "❌ ТАЙМАУТ: Превышено время ожидания ответа от сервиса бота. URL={Url}, TelegramId={TelegramId}",
                    url, telegramId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ КРИТИЧЕСКАЯ ОШИБКА: Исключение при отправке напоминания. URL={Url}, TelegramId={TelegramId}",
                    url, telegramId);
                return false;
            }
        }
    }

    // DTO для отправки данных в сервис бота
    public class ReminderNotificationDto
    {
        public long TelegramId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
    }
}

