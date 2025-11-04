using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OwnersController(AppDbContext db)
    {
        _db = db;
    }

    // POST api/owners/register
    [HttpPost("register")]
    public async Task<ActionResult<int>> Register([FromBody] OwnerRegistrationDto dto)
    {
        // Проверяем, есть ли уже пользователь с таким TelegramId
        var existingOwner = await _db.Owners
            .FirstOrDefaultAsync(o => o.TelegramId == dto.TelegramId);

        if (existingOwner != null)
        {
            // Возвращаем Id уже зарегистрированного владельца
            return Ok(existingOwner.Id);
        }

        // Создаём нового владельца
        var owner = new Owner
        {
            TelegramId = dto.TelegramId,
            TelegramNick = dto.TelegramNick
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();

        // Возвращаем Id нового владельца
        return Ok(owner.Id);
    }

    // GET api/owners/{ownerId}/pets
    [HttpGet("{ownerId}/pets")]
    public async Task<ActionResult<IEnumerable<PetSummaryDto>>> GetOwnerPets(int ownerId)
    {
        var owner = await _db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == ownerId);

        if (owner == null)
            return NotFound($"Владелец с Id {ownerId} не найден.");

        var pets = owner.Pets
            .Select(p => new PetSummaryDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();

        return Ok(pets);
    }
}

// DTOs/PetSummaryDto.cs
public class PetSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
// DTOs/OwnerRegistrationDto.cs
public class OwnerRegistrationDto
{
    public long TelegramId { get; set; }
    public string? TelegramNick { get; set; }
}

