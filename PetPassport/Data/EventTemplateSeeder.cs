// Data/EventTemplateSeeder.cs
using PetPassport.Models;

namespace PetPassport.Data
{
    public static class EventTemplateSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.EventTemplates.Any())
                return; // уже засеяно

            var templates = new List<EventTemplate>
            {
                new EventTemplate
                {
                    Title = "Ежегодная вакцинация от бешенства",
                    EventType = EventType.Vaccine,
                    Medicine = "Рабизин",
                    PeriodValue = 1,
                    PeriodUnit = PeriodUnit.Год
                },
                new EventTemplate
                {
                    Title = "Комплексная вакцинация",
                    EventType = EventType.Vaccine,
                    Medicine = "Нобивак",
                    PeriodValue = 1,
                    PeriodUnit = PeriodUnit.Год
                },
                new EventTemplate
                {
                    Title = "Обработка от блох и клещей",
                    EventType = EventType.Treatment,
                    Remedy = "Фронтлайн",
                    Parasite = "Блохи, клещи",
                    TreatmentPeriodValue = 3,
                    TreatmentPeriodUnit = PeriodUnit.Месяц
                },
                new EventTemplate
                {
                    Title = "Дегельминтизация",
                    EventType = EventType.Treatment,
                    Remedy = "Мильбемакс",
                    Parasite = "Глисты",
                    TreatmentPeriodValue = 3,
                    TreatmentPeriodUnit = PeriodUnit.Месяц
                },
                new EventTemplate
                {
                    Title = "Плановый осмотр у ветеринара",
                    EventType = EventType.DoctorVisit,
                    Clinic = "Ветеринарная клиника",
                    Doctor = "Ветеринар",
                    Recommendations = "Ежегодный профилактический осмотр"
                }
            };

            db.EventTemplates.AddRange(templates);
            await db.SaveChangesAsync();
        }
    }
}