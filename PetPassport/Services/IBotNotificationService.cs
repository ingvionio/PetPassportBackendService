namespace PetPassport.Services
{
    public interface IBotNotificationService
    {
        Task<bool> SendReminderAsync(long telegramId, string petName, string eventType, string eventTitle, DateTime eventDate);
    }
}

