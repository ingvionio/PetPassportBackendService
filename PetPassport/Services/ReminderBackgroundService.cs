using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // Проверка каждую минуту (для тестирования)

        public ReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 ReminderBackgroundService запущен. Проверка напоминаний каждые {Interval} минут", _checkInterval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при проверке напоминаний");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("ReminderBackgroundService остановлен");
        }

        private async Task CheckAndSendRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var botService = scope.ServiceProvider.GetRequiredService<IBotNotificationService>();

            var now = DateTime.UtcNow;

            // Получаем все события с необходимыми данными через JOIN
            // Загружаем только нужные поля: имя питомца и TelegramId владельца
            var eventsToRemind = await db.Events
                .Join(db.Pets, e => e.PetId, p => p.Id, (e, p) => new { Event = e, Pet = p })
                .Join(db.Owners, ep => ep.Pet.OwnerId, o => o.Id, (ep, o) => new
                {
                    Event = ep.Event,
                    PetName = ep.Pet.Name,
                    TelegramId = o.TelegramId
                })
                .Where(x => x.Event.ReminderEnabled 
                    && x.Event.ReminderDate.HasValue 
                    && x.Event.ReminderDate.Value <= now 
                    && !x.Event.IsReminderSent)
                .ToListAsync(cancellationToken);

            if (!eventsToRemind.Any())
            {
                _logger.LogInformation("🔍 Проверка напоминаний: событий для отправки не найдено (время проверки: {Now:yyyy-MM-dd HH:mm:ss} UTC)", now);
                return;
            }

            _logger.LogInformation(
                "🔔 Найдено {Count} событий для отправки напоминаний (время проверки: {Now:yyyy-MM-dd HH:mm:ss} UTC)", 
                eventsToRemind.Count, now);

            foreach (var item in eventsToRemind)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    _logger.LogInformation(
                        "Отправка напоминания: EventId={EventId}, TelegramId={TelegramId}, PetName={PetName}, EventType={EventType}",
                        item.Event.Id, item.TelegramId, item.PetName, item.Event.EventType);
                    
                    var success = await botService.SendReminderAsync(
                        (long)item.TelegramId,
                        item.PetName,
                        item.Event.EventType,
                        item.Event.Title,
                        item.Event.EventDate);

                    if (success)
                    {
                        // Загружаем событие для обновления (чтобы EF отслеживало изменения)
                        var eventToUpdate = await db.Events.FindAsync(new object[] { item.Event.Id }, cancellationToken);
                        if (eventToUpdate != null)
                        {
                            eventToUpdate.IsReminderSent = true;
                            await db.SaveChangesAsync(cancellationToken);
                            _logger.LogInformation(
                                "✅ Напоминание успешно отправлено и помечено в БД как отправленное. EventId={EventId}, PetId={PetId}, TelegramId={TelegramId}",
                                item.Event.Id, item.Event.PetId, item.TelegramId);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "⚠️ Не удалось отправить напоминание. Будет повторная попытка при следующей проверке. EventId={EventId}, PetId={PetId}, TelegramId={TelegramId}",
                            item.Event.Id, item.Event.PetId, item.TelegramId);
                        // Не помечаем как отправленное, чтобы попробовать снова при следующей проверке
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Ошибка при обработке напоминания: EventId={EventId}, PetId={PetId}",
                        item.Event.Id, item.Event.PetId);
                    // Продолжаем обработку следующих событий
                }
            }
        }
    }
}

