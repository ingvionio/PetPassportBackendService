using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
public abstract class EventControllerBase<TEvent, TDto>
    : ControllerBase
    where TEvent : PetEvent, new()
{
    protected readonly AppDbContext _db;

    protected EventControllerBase(AppDbContext db)
    {
        _db = db;
    }

    // -------- CREATE --------
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] TDto dto)
    {
        var entity = MapCreateDto(dto);
        
        // Вычисляем и сохраняем ReminderDate
        UpdateReminderDate(entity);

        _db.Events.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(entity.Id);
    }

    // -------- GET ONE --------
    [HttpGet("{id}")]
    public async Task<ActionResult<TDto>> GetById(int id)
    {
        var entity = await _db.Events
            .OfType<TEvent>()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFound();

        return Ok(MapToReturnDto(entity));
    }

    // -------- UPDATE --------
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] TDto dto)
    {
        var entity = await _db.Events
            .OfType<TEvent>()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFound();

        // Сохраняем старые значения для проверки изменений
        var oldReminderEnabled = entity.ReminderEnabled;
        var oldReminderValue = entity.ReminderValue;
        var oldReminderUnit = entity.ReminderUnit;
        var oldEventDate = entity.EventDate;

        MapUpdateDto(entity, dto);

        // Если изменились параметры напоминания или дата события, пересчитываем ReminderDate
        // и сбрасываем флаг отправки
        if (oldReminderEnabled != entity.ReminderEnabled ||
            oldReminderValue != entity.ReminderValue ||
            oldReminderUnit != entity.ReminderUnit ||
            oldEventDate != entity.EventDate)
        {
            UpdateReminderDate(entity);
            entity.IsReminderSent = false; // Сбрасываем флаг, так как напоминание изменилось
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // -------- DELETE --------
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await _db.Events
            .OfType<TEvent>()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null)
            return NotFound();

        _db.Events.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // --- абстрактные мапперы ---
    protected abstract TEvent MapCreateDto(TDto dto);
    protected abstract void MapUpdateDto(TEvent entity, TDto dto);
    protected abstract TDto MapToReturnDto(TEvent entity);

    // Вспомогательный метод для обновления ReminderDate
    private void UpdateReminderDate(TEvent entity)
    {
        entity.ReminderDate = entity.CalculateReminderDate();
    }
}

public class PetEventDto
{
    public string Title { get; set; }
    public DateTime EventDate { get; set; }

    public bool ReminderEnabled { get; set; }
    public int ReminderValue { get; set; }
    public PeriodUnit ReminderUnit { get; set; }

    public int PetId { get; set; }
}


