using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaccineController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VaccineController(AppDbContext db)
        {
            _db = db;
        }

        // Добавление новой прививки
        [HttpPost]
        public async Task<ActionResult<int>> AddVaccine([FromBody] VaccineDto dto)
        {
            var pet = await _db.Pets
                .Include(p => p.Vaccines)
                .FirstOrDefaultAsync(p => p.Id == dto.PetId);

            if (pet == null)
                return NotFound("Нет питомца с таким Id");

            var vaccine = new Vaccine
            {
                Title = dto.Title,
                VacinationDate = dto.VaccinationDate,
                PeriodValue = dto.PeriodValue,
                PeriodUnit = dto.PeriodUnit,
                NextVacinationDate = dto.NextVacinationDate,
                PetId = pet.Id,
                pet = pet
            };

            _db.Vaccines.Add(vaccine);
            pet.Vaccines.Add(vaccine);

            await _db.SaveChangesAsync();

            return Ok(vaccine.Id);
        }

        // ✅ 1. Получение всех прививок питомца
        [HttpGet("pet/{petId}")]
        public async Task<ActionResult<IEnumerable<VaccineDto>>> GetVaccinesByPetId(int petId)
        {
            var pet = await _db.Pets.Include(p => p.Vaccines).FirstOrDefaultAsync(p => p.Id == petId);
            if (pet == null)
                return NotFound("Питомец не найден.");

            var vaccines = pet.Vaccines.Select(v => new VaccineDto
            {
                Title = v.Title,
                VaccinationDate = v.VacinationDate,
                PeriodValue = v.PeriodValue,
                PeriodUnit = v.PeriodUnit,
                NextVacinationDate = v.NextVacinationDate,
                PetId = v.PetId
            }).ToList();

            return Ok(vaccines);
        }

        // ✅ 2. Редактирование прививки
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateVaccine(int id, [FromBody] VaccineRedDto dto)
        {
            var vaccine = await _db.Vaccines.FirstOrDefaultAsync(v => v.Id == id);
            if (vaccine == null)
                return NotFound("Прививка не найдена.");

            vaccine.Title = dto.Title;
            vaccine.VacinationDate = (DateOnly)dto.VaccinationDate;
            vaccine.PeriodValue = dto.PeriodValue;
            vaccine.PeriodUnit = dto.PeriodUnit;
            vaccine.NextVacinationDate = dto.NextVacinationDate;

            await _db.SaveChangesAsync();

            return NoContent(); // стандартный ответ при успешном обновлении
        }
    }

    // DTO для передачи данных
    public class VaccineDto
    {
        public string Title { get; set; }
        public DateOnly VaccinationDate { get; set; }
        public int? PeriodValue { get; set; }
        public PeriodUnit? PeriodUnit { get; set; }
        public DateOnly? NextVacinationDate { get; set; }
        public int PetId { get; set; }
    }

    public class VaccineRedDto : VaccineDto
    {
        public string? Title { get; set; }

        public DateOnly? VaccinationDate { get; set; }
    }
}
