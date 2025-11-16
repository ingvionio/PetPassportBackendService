using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/treatment")]
public class TreatmentController : EventControllerBase<TreatmentEvent, TreatmentDto>
{
    public TreatmentController(AppDbContext db) : base(db) { }

    // --- CREATE ---
    [HttpPost("create")]
    protected override TreatmentEvent MapCreateDto(TreatmentDto dto)
    {
        return new TreatmentEvent
        {
            PetId = dto.PetId,
            Title = dto.Title,
            EventDate = dto.EventDate,

            Remedy = dto.Remedy,
            Parasite = dto.Parasite,

            PeriodValue = dto.PeriodValue,
            PeriodUnit = dto.PeriodUnit,
            NextTreatmentDate = dto.NextTreatmentDate,

            ReminderEnabled = dto.ReminderEnabled,
            ReminderValue = dto.ReminderValue,
            ReminderUnit = dto.ReminderUnit
        };
    }

    // --- UPDATE ---
    [HttpPut("{id}")]
    protected override void MapUpdateDto(TreatmentEvent entity, TreatmentDto dto)
    {
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.EventDate != null) entity.EventDate = dto.EventDate;

        if (dto.Remedy != null) entity.Remedy = dto.Remedy;
        if (dto.Parasite != null) entity.Parasite = dto.Parasite;

        if (dto.PeriodValue != null) entity.PeriodValue = dto.PeriodValue;
        if (dto.PeriodUnit != null) entity.PeriodUnit = dto.PeriodUnit;
        if (dto.NextTreatmentDate != null) entity.NextTreatmentDate = dto.NextTreatmentDate;

        if (dto.ReminderEnabled != null) entity.ReminderEnabled = dto.ReminderEnabled;
        if (dto.ReminderValue != null) entity.ReminderValue = dto.ReminderValue;
        if (dto.ReminderUnit != null) entity.ReminderUnit = dto.ReminderUnit;
    }

    // --- GET ONE ---
    [HttpGet("{id}")]
    protected override TreatmentDto MapToReturnDto(TreatmentEvent t)
    {
        return new TreatmentDto
        {
            Title = t.Title,
            EventDate = t.EventDate,

            Remedy = t.Remedy,
            Parasite = t.Parasite,

            PeriodValue = t.PeriodValue,
            PeriodUnit = t.PeriodUnit,
            NextTreatmentDate = t.NextTreatmentDate,

            ReminderEnabled = t.ReminderEnabled,
            ReminderValue = (int)t.ReminderValue,
            ReminderUnit = (PeriodUnit)t.ReminderUnit
        };
    }
}

public class TreatmentDto : PetEventDto
{
    public string? Remedy { get; set; }
    public string? Parasite { get; set; }

    public int? PeriodValue { get; set; }
    public PeriodUnit? PeriodUnit { get; set; }
    public DateTime? NextTreatmentDate { get; set; }
}

