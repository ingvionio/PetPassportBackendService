using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PetsController(AppDbContext db)
    {
        _db = db;
    }

    // POST api/pets
    [HttpPost]
    public async Task<ActionResult<int>> CreatePet([FromBody] PetCreateDto dto)
    {
        // Проверяем, что владелец существует
        var owner = await _db.Owners
            .Include(o => o.Pets) // чтобы обновить список питомцев
            .FirstOrDefaultAsync(o => o.Id == dto.OwnerId);

        if (owner == null)
            return NotFound($"Владелец с Id {dto.OwnerId} не найден.");

        // Создаём нового питомца
        var pet = new Pet
        {
            Name = dto.Name,
            Breed = dto.Breed,
            WeightKg = dto.WeightKg,
            BirthDate = dto.BirthDate,
            OwnerId = owner.Id,
            Owner = owner
        };

        _db.Pets.Add(pet);
        // Добавляем питомца в коллекцию владельца
        owner.Pets.Add(pet);

        await _db.SaveChangesAsync();

        // Возвращаем Id нового питомца
        return Ok(pet.Id);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<PetDto>> GetPet(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null) return NotFound();

        var dto = new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Breed = pet.Breed,
            WeightKg = pet.WeightKg,
            BirthDate = pet.BirthDate,
            OwnerId = pet.OwnerId
        };

        return Ok(dto);
    }
}


// DTOs/PetCreateDto.cs
public class PetCreateDto
{
    public string Name { get; set; } = null!;
    public string? Breed { get; set; }
    public decimal? WeightKg { get; set; }
    public DateTime? BirthDate { get; set; }
    public int OwnerId { get; set; } // привязка к владельцу
}

public class PetDto : PetCreateDto
{
    public int Id { get; set; }
}
