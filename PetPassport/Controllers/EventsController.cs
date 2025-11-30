using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    // Получение предстоящих процедур для питомца
    [HttpGet("upcoming/{petId}")]
    public async Task<ActionResult<List<UpcomingEventDto>>> GetUpcomingEvents(int petId)
    {
        var now = DateTime.UtcNow;

        // Получаем все предстоящие события всех типов для питомца
        var doctorVisits = await _db.Events
            .OfType<DoctorVisitEvent>()
            .Where(e => e.PetId == petId && e.EventDate >= now)
            .Select(e => new UpcomingEventDto
            {
                Id = e.Id,
                Type = "doctor-visit",
                Title = e.Title,
                EventDate = e.EventDate,
                ReminderEnabled = e.ReminderEnabled,
                Clinic = e.Clinic,
                Doctor = e.Doctor
            })
            .ToListAsync();

        var vaccines = await _db.Events
            .OfType<VaccineEvent>()
            .Where(e => e.PetId == petId && e.EventDate >= now)
            .Select(e => new UpcomingEventDto
            {
                Id = e.Id,
                Type = "vaccine",
                Title = e.Title,
                EventDate = e.EventDate,
                ReminderEnabled = e.ReminderEnabled,
                Medicine = e.Medicine,
                NextVaccinationDate = e.NextVaccinationDate
            })
            .ToListAsync();

        var treatments = await _db.Events
            .OfType<TreatmentEvent>()
            .Where(e => e.PetId == petId && e.EventDate >= now)
            .Select(e => new UpcomingEventDto
            {
                Id = e.Id,
                Type = "treatment",
                Title = e.Title,
                EventDate = e.EventDate,
                ReminderEnabled = e.ReminderEnabled,
                Remedy = e.Remedy,
                Parasite = e.Parasite,
                NextTreatmentDate = e.NextTreatmentDate
            })
            .ToListAsync();

        // Объединяем все события и сортируем по дате
        var allEvents = doctorVisits
            .Concat(vaccines)
            .Concat(treatments)
            .OrderBy(e => e.EventDate)
            .ToList();

        return Ok(allEvents);
    }
}

// DTO для предстоящих событий
public class UpcomingEventDto
{
    public int Id { get; set; }
    public string Type { get; set; } // "doctor-visit", "vaccine", "treatment"
    public string Title { get; set; }
    public DateTime EventDate { get; set; }
    public bool ReminderEnabled { get; set; }

    // Поля для посещения врача
    public string? Clinic { get; set; }
    public string? Doctor { get; set; }

    // Поля для вакцинации
    public string? Medicine { get; set; }
    public DateTime? NextVaccinationDate { get; set; }

    // Поля для обработки
    public string? Remedy { get; set; }
    public string? Parasite { get; set; }
    public DateTime? NextTreatmentDate { get; set; }
}