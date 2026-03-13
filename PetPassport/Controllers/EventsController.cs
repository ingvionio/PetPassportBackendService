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

    // GET /api/events/{petId}?status=Upcoming
    [HttpGet("{petId}")]
    public async Task<ActionResult<List<EventsDto>>> GetEvents(
        int petId,
        [FromQuery] EventStatus? status = null)
    {
        var doctorVisits = await _db.Events
            .OfType<DoctorVisitEvent>()
            .Where(e => e.PetId == petId && (status == null || e.Status == status))
            .Select(e => new EventsDto
            {
                Id = e.Id,
                Type = "doctor-visit",
                Title = e.Title,
                EventDate = e.EventDate,
                Status = e.Status,
                ReminderEnabled = e.ReminderEnabled,
                Clinic = e.Clinic,
                Doctor = e.Doctor
            })
            .ToListAsync();

        var vaccines = await _db.Events
            .OfType<VaccineEvent>()
            .Where(e => e.PetId == petId && (status == null || e.Status == status))
            .Select(e => new EventsDto
            {
                Id = e.Id,
                Type = "vaccine",
                Title = e.Title,
                EventDate = e.EventDate,
                Status = e.Status,
                ReminderEnabled = e.ReminderEnabled,
                Medicine = e.Medicine,
                NextVaccinationDate = e.NextVaccinationDate
            })
            .ToListAsync();

        var treatments = await _db.Events
            .OfType<TreatmentEvent>()
            .Where(e => e.PetId == petId && (status == null || e.Status == status))
            .Select(e => new EventsDto
            {
                Id = e.Id,
                Type = "treatment",
                Title = e.Title,
                EventDate = e.EventDate,
                Status = e.Status,
                ReminderEnabled = e.ReminderEnabled,
                Remedy = e.Remedy,
                Parasite = e.Parasite,
                NextTreatmentDate = e.NextTreatmentDate
            })
            .ToListAsync();

        var allEvents = doctorVisits
            .Concat(vaccines)
            .Concat(treatments)
            .OrderBy(e => e.EventDate)
            .ToList();

        return Ok(allEvents);
    }
    // PATCH /api/events/{id}/status
    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateEventStatusDto dto)
    {
        var entity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFound();

        entity.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/events/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFound();

        _db.Events.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}


// DTO для событий
public class EventsDto
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
    public EventStatus Status { get; set; }
}