using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using PetPassport.Data;
using PetPassport.Models;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public PetsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpPost("{petId}/upload")]
    public async Task<IActionResult> UploadPhoto(int petId, IFormFile file, [FromQuery] string? telegramFileId = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не найден.");

        var pet = await _db.Pets.Include(p => p.Photos).FirstOrDefaultAsync(p => p.Id == petId);
        if (pet == null)
            return NotFound($"Питомец с Id {petId} не найден.");

        // Папка для хранения файлов
        var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", petId.ToString());
        Directory.CreateDirectory(uploadFolder);

        // Уникальное имя файла
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        // Сохраняем файл на диск
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Относительный путь (для БД)
        var relativeUrl = $"/uploads/pets/{petId}/{fileName}";

        // Создаём запись в БД
        var petPhoto = new PetPhoto
        {
            Url = relativeUrl,
            TelegramFileId = telegramFileId,
            PetId = pet.Id
        };

        _db.PetPhotos.Add(petPhoto);
        await _db.SaveChangesAsync();

        return Ok(new { photoUrl = relativeUrl, petPhoto.Id });
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
        var pet = await _db.Pets
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null)
            return NotFound();

        var dto = new PetDto
        {
            Id = pet.Id,
            Name = pet.Name,
            Breed = pet.Breed,
            WeightKg = pet.WeightKg,
            BirthDate = pet.BirthDate,
            OwnerId = pet.OwnerId,
            Photos = pet.Photos.Select(photo => new PetPhotoDto
            {
                Id = photo.Id,
                Url = photo.Url,
                TelegramFileId = photo.TelegramFileId
            }).ToList()
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
    public List<PetPhotoDto> Photos { get; set; } = new();
}
public class PetPhotoDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? TelegramFileId { get; set; }
}
