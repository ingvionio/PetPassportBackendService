using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/event-templates")]
public class EventTemplatesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventTemplatesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/event-templates
    [HttpGet]
    public async Task<ActionResult<List<EventTemplateDto>>> GetAll()
    {
        var templates = await _db.EventTemplates
            .Select(t => new EventTemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                EventType = t.EventType,
                Clinic = t.Clinic,
                Doctor = t.Doctor,
                Diagnosis = t.Diagnosis,
                Recommendations = t.Recommendations,
                Referrals = t.Referrals,
                Medicine = t.Medicine,
                PeriodValue = t.PeriodValue,
                PeriodUnit = t.PeriodUnit,
                Remedy = t.Remedy,
                Parasite = t.Parasite,
                TreatmentPeriodValue = t.TreatmentPeriodValue,
                TreatmentPeriodUnit = t.TreatmentPeriodUnit
            })
            .ToListAsync();

        return Ok(templates);
    }

    // GET /api/event-templates/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<EventTemplateDto>> GetById(int id)
    {
        var template = await _db.EventTemplates.FindAsync(id);

        if (template == null)
            return NotFound();

        return Ok(new EventTemplateDto
        {
            Id = template.Id,
            Title = template.Title,
            EventType = template.EventType,
            Clinic = template.Clinic,
            Doctor = template.Doctor,
            Diagnosis = template.Diagnosis,
            Recommendations = template.Recommendations,
            Referrals = template.Referrals,
            Medicine = template.Medicine,
            PeriodValue = template.PeriodValue,
            PeriodUnit = template.PeriodUnit,
            Remedy = template.Remedy,
            Parasite = template.Parasite,
            TreatmentPeriodValue = template.TreatmentPeriodValue,
            TreatmentPeriodUnit = template.TreatmentPeriodUnit
        });
    }

    // GET /api/event-templates/by-type/{eventType}
    [HttpGet("by-type/{eventType}")]
    public async Task<ActionResult<List<EventTemplateDto>>> GetByType(EventType eventType)
    {
        var templates = await _db.EventTemplates
            .Where(t => t.EventType == eventType)
            .Select(t => new EventTemplateDto
            {
                Id = t.Id,
                Title = t.Title,
                EventType = t.EventType,
                Clinic = t.Clinic,
                Doctor = t.Doctor,
                Diagnosis = t.Diagnosis,
                Recommendations = t.Recommendations,
                Referrals = t.Referrals,
                Medicine = t.Medicine,
                PeriodValue = t.PeriodValue,
                PeriodUnit = t.PeriodUnit,
                Remedy = t.Remedy,
                Parasite = t.Parasite,
                TreatmentPeriodValue = t.TreatmentPeriodValue,
                TreatmentPeriodUnit = t.TreatmentPeriodUnit
            })
            .ToListAsync();

        return Ok(templates);
    }
}

public class EventTemplateDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public EventType EventType { get; set; }

    // DoctorVisit
    public string? Clinic { get; set; }
    public string? Doctor { get; set; }
    public string? Diagnosis { get; set; }
    public string? Recommendations { get; set; }
    public string? Referrals { get; set; }

    // Vaccine
    public string? Medicine { get; set; }
    public int? PeriodValue { get; set; }
    public PeriodUnit? PeriodUnit { get; set; }

    // Treatment
    public string? Remedy { get; set; }
    public string? Parasite { get; set; }
    public int? TreatmentPeriodValue { get; set; }
    public PeriodUnit? TreatmentPeriodUnit { get; set; }
}