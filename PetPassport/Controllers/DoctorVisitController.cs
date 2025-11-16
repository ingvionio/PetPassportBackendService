using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/doctor-visit")]
public class DoctorVisitController : EventControllerBase<DoctorVisitEvent, DoctorVisitDto>
{
    public DoctorVisitController(AppDbContext db) : base(db) { }

    // ---------- CREATE ----------
    [HttpPost("create")]
    protected override DoctorVisitEvent MapCreateDto(DoctorVisitDto dto)
    {
        return new DoctorVisitEvent
        {
            PetId = dto.PetId,
            Title = dto.Title,
            EventDate = dto.EventDate,

            Clinic = dto.Clinic!,
            Doctor = dto.Doctor!,
            Diagnosis = dto.Diagnosis,
            Recommendations = dto.Recommendations,
            Referrals = dto.Referrals,

            ReminderEnabled = dto.ReminderEnabled,
            ReminderValue = dto.ReminderValue,
            ReminderUnit = dto.ReminderUnit
        };
    }

    // ---------- UPDATE ----------
    [HttpPut("{id}")]
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

    // ---------- RETURN ----------
    [HttpGet("{petId}")]
    protected override DoctorVisitDto MapToReturnDto(DoctorVisitEvent e)
    {
        return new DoctorVisitDto
        {
            PetId = e.PetId,
            Title = e.Title,
            EventDate = e.EventDate,

            Clinic = e.Clinic,
            Doctor = e.Doctor,
            Diagnosis = e.Diagnosis,
            Recommendations = e.Recommendations,
            Referrals = e.Referrals,

            ReminderEnabled = e.ReminderEnabled,
            ReminderValue = (int)e.ReminderValue,
            ReminderUnit = (PeriodUnit)e.ReminderUnit
        };
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
