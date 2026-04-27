using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers.V2
{
    [Authorize]
    [ApiController]
    [Route("api/v2/events")]
    public class EventsV2Controller : ControllerBase
    {
        private readonly AppDbContext _db;

        public EventsV2Controller(AppDbContext db)
        {
            _db = db;
        }

        private int CurrentOwnerId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private async Task<bool> PetBelongsToCurrentOwner(int petId) =>
            await _db.Pets.AnyAsync(p => p.Id == petId && p.OwnerId == CurrentOwnerId);

        /// <summary>Список событий питомца</summary>
        /// <remarks>
        /// Фильтрация по статусу через query-параметр status (передавать строкой):
        /// - Upcoming — предстоящие
        /// - Indefinite — неопределённые (дата прошла, статус не проставлен вручную)
        /// - Completed — выполненные
        /// - Cancelled — отменённые
        /// </remarks>
        [HttpGet("{petId}")]
        public async Task<ActionResult<List<EventsDto>>> GetEvents(
            int petId,
            [FromQuery] EventStatus? status = null)
        {
            if (!await PetBelongsToCurrentOwner(petId))
                return Forbid();

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

        /// <summary>Обновить статус события</summary>
        /// <remarks>Статусы: Upcoming, Indefinite, Completed, Cancelled</remarks>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateEventStatusDto dto)
        {
            var entity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
                return NotFound();

            if (!await PetBelongsToCurrentOwner(entity.PetId))
                return Forbid();

            entity.Status = dto.Status;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Удалить событие</summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var entity = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
                return NotFound();

            if (!await PetBelongsToCurrentOwner(entity.PetId))
                return Forbid();

            _db.Events.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
