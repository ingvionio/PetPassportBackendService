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

        if (pet.Photos.Count >= 4)
            return BadRequest("Превышен лимит: у питомца может быть не более 4 фотографий.");

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

        if (owner.Pets.Count >= 4)
            return BadRequest("Превышен лимит: у владельца не может быть больше 4 питомцев.");

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(int id, [FromBody] PetUpdateDto dto)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null)
            return NotFound($"Питомец с Id {id} не найден.");

        // Обновляем только переданные поля
        if (!string.IsNullOrWhiteSpace(dto.Name))
            pet.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Breed))
            pet.Breed = dto.Breed;

        if (dto.WeightKg.HasValue)
            pet.WeightKg = dto.WeightKg;

        if (dto.BirthDate.HasValue)
            pet.BirthDate = dto.BirthDate;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Информация о питомце обновлена успешно." });
    }

    [HttpPut("{petId}/photos")]
    public async Task<IActionResult> UpdatePetPhotos(int petId,
    [FromForm] List<IFormFile>? newFiles,
    [FromForm] List<int>? deletePhotoIds)
    {
        var pet = await _db.Pets.Include(p => p.Photos)
                                .FirstOrDefaultAsync(p => p.Id == petId);

        if (pet == null)
            return NotFound($"Питомец с Id {petId} не найден.");

        // 🔹 1. Удаляем указанные фото
        if (deletePhotoIds != null && deletePhotoIds.Any())
        {
            var photosToDelete = pet.Photos.Where(p => deletePhotoIds.Contains(p.Id)).ToList();
            foreach (var photo in photosToDelete)
            {
                // Удаляем файл с диска
                var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", photo.Url.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                _db.PetPhotos.Remove(photo);
            }
        }

        // 🔹 2. Добавляем новые фото (если не превышен лимит)
        if (newFiles != null && newFiles.Any())
        {
            int currentCount = pet.Photos.Count - (deletePhotoIds?.Count ?? 0);
            int availableSlots = 4 - currentCount;

            if (newFiles.Count > availableSlots)
                return BadRequest($"Можно добавить максимум {availableSlots} новых фото (лимит — 4).");

            var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", petId.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in newFiles)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var relativeUrl = $"/uploads/pets/{petId}/{fileName}";
                _db.PetPhotos.Add(new PetPhoto { Url = relativeUrl, PetId = pet.Id });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Фотографии питомца успешно обновлены." });
    }

    // DELETE api/pets/{petId}/photos/{photoId}
    [HttpDelete("{petId}/photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(int petId, int photoId)
    {
        var pet = await _db.Pets
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == petId);

        if (pet == null)
            return NotFound($"Питомец с Id {petId} не найден.");

        var photo = pet.Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
            return NotFound($"Фотография с Id {photoId} не найдена у питомца {petId}.");

        // Удаляем файл с диска
        var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", photo.Url.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но продолжаем удаление записи из БД
                // Можно добавить ILogger для логирования
            }
        }

        // Удаляем запись из БД
        _db.PetPhotos.Remove(photo);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Фотография успешно удалена." });
    }

    // DELETE api/pets/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        var pet = await _db.Pets
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null)
            return NotFound($"Питомец с Id {id} не найден.");

        // Удаляем все фотографии питомца с диска
        foreach (var photo in pet.Photos)
        {
            var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", photo.Url.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch
                {
                    // Игнорируем ошибки удаления файлов
                }
            }
        }

        // Пытаемся удалить папку питомца, если она пустая
        try
        {
            var petFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", id.ToString());
            if (Directory.Exists(petFolder) && !Directory.EnumerateFileSystemEntries(petFolder).Any())
            {
                Directory.Delete(petFolder);
            }
        }
        catch
        {
            // Игнорируем ошибки удаления папки
        }

        // Удаляем питомца (фото и события удалятся каскадно)
        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Питомец успешно удалён." });
    }
}


public class PetUpdateDto
{
    public string? Name { get; set; }
    public string? Breed { get; set; }
    public decimal? WeightKg { get; set; }
    public DateOnly? BirthDate { get; set; }
}

// DTOs/PetCreateDto.cs
public class PetCreateDto
{
    public string Name { get; set; } = null!;
    public string? Breed { get; set; }
    public decimal? WeightKg { get; set; }
    public DateOnly? BirthDate { get; set; }
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
