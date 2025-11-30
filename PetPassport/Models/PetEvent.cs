namespace PetPassport.Models
{
    public abstract class PetEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }

        public int PetId { get; set; }

        public bool ReminderEnabled { get; set; } = false;
        public int? ReminderValue { get; set; }
        public PeriodUnit? ReminderUnit { get; set; }
        public string EventType { get; set; } = string.Empty;

        // Хранимое поле в БД для напоминаний
        public DateTime? ReminderDate { get; set; }
        
        // Флаг, что напоминание уже отправлено
        public bool IsReminderSent { get; set; } = false;

        // Вычисляемое свойство для обратной совместимости (если ReminderDate не установлен)
        public DateTime? CalculateReminderDate()
        {
            if (!ReminderEnabled || !ReminderValue.HasValue || !ReminderUnit.HasValue)
                return null;

            return ReminderUnit switch
            {
                PeriodUnit.День => EventDate.AddDays(-ReminderValue.Value),
                PeriodUnit.Месяц => EventDate.AddMonths(-ReminderValue.Value),
                PeriodUnit.Год => EventDate.AddYears(-ReminderValue.Value),
                _ => null
            };
        }
    }
}
