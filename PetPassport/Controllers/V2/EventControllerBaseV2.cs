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
    public abstract class EventControllerBaseV2<TEvent, TDto>
        : ControllerBase
        where TEvent : PetEvent, new()
    {
        protected readonly AppDbContext _db;

        protected EventControllerBaseV2(AppDbContext db)
        {
            _db = db;
        }

        protected int CurrentOwnerId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        protected async Task<bool> PetBelongsToCurrentOwner(int petId) =>
            await _db.Pets.AnyAsync(p => p.Id == petId && p.OwnerId == CurrentOwnerId);

        // POST
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] TDto dto)
        {
            var entity = MapCreateDto(dto);

            if (!await PetBelongsToCurrentOwner(entity.PetId))
                return Forbid();

            entity.Status = entity.EventDate > DateTime.UtcNow
                ? EventStatus.Upcoming
                : EventStatus.Indefinite;

            entity.ReminderDate = entity.CalculateReminderDate();

            _db.Events.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(entity.Id);
        }

        // GET /{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TDto>> GetById(int id)
        {
            var entity = await _db.Events
                .OfType<TEvent>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return NotFound();

            if (!await PetBelongsToCurrentOwner(entity.PetId))
                return Forbid();

            return Ok(MapToReturnDto(entity));
        }

        // PUT /{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TDto dto)
        {
            var entity = await _db.Events
                .OfType<TEvent>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return NotFound();

            if (!await PetBelongsToCurrentOwner(entity.PetId))
                return Forbid();

            var oldReminderEnabled = entity.ReminderEnabled;
            var oldReminderValue = entity.ReminderValue;
            var oldReminderUnit = entity.ReminderUnit;
            var oldEventDate = entity.EventDate;

            MapUpdateDto(entity, dto);

            if (oldReminderEnabled != entity.ReminderEnabled ||
                oldReminderValue != entity.ReminderValue ||
                oldReminderUnit != entity.ReminderUnit ||
                oldEventDate != entity.EventDate)
            {
                entity.ReminderDate = entity.CalculateReminderDate();
                entity.IsReminderSent = false;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        protected abstract TEvent MapCreateDto(TDto dto);
        protected abstract void MapUpdateDto(TEvent entity, TDto dto);
        protected abstract TDto MapToReturnDto(TEvent entity);

        protected void MapBaseDto(TEvent entity, PetEventDto dto)
        {
            entity.PetId = dto.PetId;
            entity.Title = dto.Title;
            entity.EventDate = dto.EventDate;
            entity.ReminderEnabled = dto.ReminderEnabled;
            entity.ReminderValue = dto.ReminderValue;
            entity.ReminderUnit = dto.ReminderUnit;
        }
    }
}
