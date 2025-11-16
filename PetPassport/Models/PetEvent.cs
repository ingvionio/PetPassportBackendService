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

        public DateTime? ReminderDate
        {
            get
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
}
