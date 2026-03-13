using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/vaccine")]
public class VaccineController : EventControllerBase<VaccineEvent, VaccineDto>
{
    public VaccineController(AppDbContext db) : base(db) { }

    protected override VaccineEvent MapCreateDto(VaccineDto dto)
    {
        var entity = new VaccineEvent
        {
            Medicine = dto.Medicine,
            PeriodValue = dto.PeriodValue,
            PeriodUnit = dto.PeriodUnit,
            NextVaccinationDate = dto.NextVaccinationDate
        };
        MapBaseDto(entity, dto);
        return entity;
    }

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

    protected override VaccineDto MapToReturnDto(VaccineEvent v)
    {
        var dto = new VaccineDto
        {
            Medicine = v.Medicine,
            PeriodValue = v.PeriodValue,
            PeriodUnit = v.PeriodUnit,
            NextVaccinationDate = v.NextVaccinationDate
        };
        dto.MapFromEntity(v);
        return dto;
    }
}

public class VaccineDto : PetEventDto
{
    public string? Medicine { get; set; }
    public int? PeriodValue { get; set; }
    public PeriodUnit? PeriodUnit { get; set; }
    public DateTime? NextVaccinationDate { get; set; }
}


