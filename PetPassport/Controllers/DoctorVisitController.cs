using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/doctor-visit")]
public class DoctorVisitController : EventControllerBase<DoctorVisitEvent, DoctorVisitDto>
{
    public DoctorVisitController(AppDbContext db) : base(db) { }

    protected override DoctorVisitEvent MapCreateDto(DoctorVisitDto dto)
    {
        var entity = new DoctorVisitEvent
        {
            Clinic = dto.Clinic!,
            Doctor = dto.Doctor!,
            Diagnosis = dto.Diagnosis,
            Recommendations = dto.Recommendations,
            Referrals = dto.Referrals
        };
        MapBaseDto(entity, dto);
        return entity;
    }

    protected override void MapUpdateDto(DoctorVisitEvent entity, DoctorVisitDto dto)
    {
        if (dto.Title != null) entity.Title = dto.Title;
        if (dto.EventDate != null) entity.EventDate = dto.EventDate;
        if (dto.Clinic != null) entity.Clinic = dto.Clinic;
        if (dto.Doctor != null) entity.Doctor = dto.Doctor;
        if (dto.Diagnosis != null) entity.Diagnosis = dto.Diagnosis;
        if (dto.Recommendations != null) entity.Recommendations = dto.Recommendations;
        if (dto.Referrals != null) entity.Referrals = dto.Referrals;
        if (dto.ReminderEnabled != null) entity.ReminderEnabled = dto.ReminderEnabled;
        if (dto.ReminderValue != null) entity.ReminderValue = dto.ReminderValue;
        if (dto.ReminderUnit != null) entity.ReminderUnit = dto.ReminderUnit;
    }

    protected override DoctorVisitDto MapToReturnDto(DoctorVisitEvent e)
    {
        var dto = new DoctorVisitDto
        {
            Clinic = e.Clinic,
            Doctor = e.Doctor,
            Diagnosis = e.Diagnosis,
            Recommendations = e.Recommendations,
            Referrals = e.Referrals
        };
        dto.MapFromEntity(e);
        return dto;
    }
}

public class DoctorVisitDto : PetEventDto
{
    public string? Clinic { get; set; }
    public string? Doctor { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }
    public string? Referrals { get; set; }
}