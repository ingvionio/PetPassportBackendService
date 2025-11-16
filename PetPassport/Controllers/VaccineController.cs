using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/vaccine")]
public class VaccineController: EventControllerBase<VaccineEvent, VaccineDto>
{
    public VaccineController(AppDbContext db) : base(db) { }

    [HttpPost("create")]
    protected override VaccineEvent MapCreateDto(VaccineDto dto)
    {
        return new VaccineEvent
        {
            PetId = dto.PetId,
            Title = dto.Title,
            Medicine = dto.Medicine,
            EventDate = dto.EventDate,
            PeriodValue = dto.PeriodValue,
            PeriodUnit = dto.PeriodUnit,
            NextVaccinationDate = dto.NextVaccinationDate,

            ReminderEnabled = dto.ReminderEnabled,
            ReminderValue = dto.ReminderValue,
            ReminderUnit = dto.ReminderUnit
        };
    }

    [HttpPut("{petId}")]
    protected override void MapUpdateDto(VaccineEvent entity, VaccineDto dto)
    {
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.Medicine != null) entity.Medicine = dto.Medicine;

        if (dto.EventDate != null) entity.EventDate = dto.EventDate;

        if (dto.PeriodValue != null) entity.PeriodValue = dto.PeriodValue;
        if (dto.PeriodUnit != null) entity.PeriodUnit = dto.PeriodUnit;
        if (dto.NextVaccinationDate != null) entity.NextVaccinationDate = dto.NextVaccinationDate;

        if (dto.ReminderEnabled != null) entity.ReminderEnabled = dto.ReminderEnabled;
        if (dto.ReminderValue != null) entity.ReminderValue = dto.ReminderValue;
        if (dto.ReminderUnit != null) entity.ReminderUnit = dto.ReminderUnit;
    }


    [HttpGet("{id}")]
    protected override VaccineDto MapToReturnDto(VaccineEvent v)
    {
        return new VaccineDto
        {
            Title = v.Title,
            Medicine = v.Medicine,
            EventDate = v.EventDate,
            PeriodValue = v.PeriodValue,
            PeriodUnit = v.PeriodUnit,
            NextVaccinationDate = v.NextVaccinationDate,

            ReminderEnabled = v.ReminderEnabled,
            ReminderValue = (int)v.ReminderValue,
            ReminderUnit = (PeriodUnit)v.ReminderUnit
        };
    }
}

public class VaccineDto : PetEventDto
{
    public string? Medicine { get; set; }
    public int? PeriodValue { get; set; }
    public PeriodUnit? PeriodUnit { get; set; }
    public DateTime? NextVaccinationDate { get; set; }
}


