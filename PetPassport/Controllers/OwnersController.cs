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

    // POST api/owners/register-login
    [HttpPost("register-login")]
    public async Task<ActionResult<int>> RegisterByLogin(
        [FromBody] OwnerLoginRegisterDto dto)
    {
        var exists = await _db.Owners
            .AnyAsync(o => o.login == dto.Login);

        if (exists)
            return BadRequest("Пользователь с таким логином уже существует");

        var owner = new Owner
        {
            login = dto.Login,
            password = dto.Password // ⚠ пока без хэша
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();

        return Ok(owner.Id);
    }

    // POST api/owners/login
    [HttpPost("login")]
    public async Task<ActionResult<OwnerLoginResultDto>> Login(
        [FromBody] OwnerLoginRegisterDto dto)
    {
        var owner = await _db.Owners.FirstOrDefaultAsync(o =>
            o.login == dto.Login &&
            o.password == dto.Password
        );

        if (owner == null)
            return Unauthorized("Неверный логин или пароль");

        return Ok(new OwnerLoginResultDto
        {
            OwnerId = owner.Id
        });
    }

    // GET api/owners/by-login/{login}
    [HttpGet("by-login/{login}")]
    public async Task<ActionResult<int>> GetOwnerIdByLogin(string login)
    {
        var owner = await _db.Owners
            .FirstOrDefaultAsync(o => o.login == login);

        if (owner == null)
            return NotFound("Пользователь не найден");

        return Ok(owner.Id);
    }


    // POST api/owners/register
    [HttpPost("register")]
    public async Task<ActionResult<int>> Register([FromBody] OwnerRegistrationDto dto)
    {
        // Проверяем, есть ли уже пользователь с таким TelegramId
        var existingOwner = await _db.Owners.FirstOrDefaultAsync(o => o.TelegramId == dto.TelegramId);

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

    // GET api/owners/by-telegram/{telegramId}
    [HttpGet("by-telegram/{telegramId}")]
    public async Task<ActionResult<OwnerWithPetsDto>> GetOwnerByTelegramId(long telegramId)
    {
        var owner = await _db.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.TelegramId == telegramId);

        if (owner == null)
            return NotFound($"Владелец с TelegramId {telegramId} не найден.");

        var dto = new OwnerWithPetsDto
        {
            OwnerId = owner.Id,
            TelegramId = (long)owner.TelegramId,
            TelegramNick = owner.TelegramNick,
            Pets = owner.Pets
                .Select(p => new PetSummaryDto { Id = p.Id, Name = p.Name })
                .ToList()
        };

        return Ok(dto);
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
// DTOs/OwnerWithPetsDto.cs
public class OwnerWithPetsDto
{
    public int OwnerId { get; set; } 
    public long TelegramId { get; set; }
    public string? TelegramNick { get; set; }
    public List<PetSummaryDto> Pets { get; set; } = new();
}

public class OwnerLoginRegisterDto
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class OwnerLoginResultDto
{
    public int OwnerId { get; set; }
}



