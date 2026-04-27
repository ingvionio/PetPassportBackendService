using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers.V2
{
    [Authorize]
    [ApiController]
    [Route("api/v2/pets")]
    public class PetsV2Controller : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PetsV2Controller(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int CurrentOwnerId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Все питомцы текущего пользователя</summary>
        [HttpGet]
        public async Task<ActionResult<List<PetDto>>> GetMyPets()
        {
            var pets = await _db.Pets
                .Include(p => p.Photos)
                .Where(p => p.OwnerId == CurrentOwnerId)
                .ToListAsync();

            return Ok(pets.Select(MapToDto).ToList());
        }

        /// <summary>Получить питомца по ID</summary>
        /// <remarks>Возвращает 403 если питомец принадлежит другому пользователю.</remarks>
        [HttpGet("{id}")]
        public async Task<ActionResult<PetDto>> GetPet(int id)
        {
            var pet = await _db.Pets
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            return Ok(MapToDto(pet));
        }

        /// <summary>Создать питомца</summary>
        /// <remarks>OwnerId берётся из токена автоматически. Лимит — 4 питомца на аккаунт. Можно сразу загрузить до 4 фото.</remarks>
        [HttpPost]
        public async Task<ActionResult<int>> CreatePet([FromForm] PetCreateV2Dto dto)
        {
            var owner = await _db.Owners
                .Include(o => o.Pets)
                .FirstOrDefaultAsync(o => o.Id == CurrentOwnerId);

            if (owner is null)
                return NotFound("Владелец не найден");

            if (owner.Pets.Count >= 4)
                return BadRequest("Превышен лимит: у владельца не может быть больше 4 питомцев.");

            var pet = new Pet
            {
                Name = dto.Name,
                Breed = dto.Breed,
                WeightKg = dto.WeightKg,
                BirthDate = dto.BirthDate,
                OwnerId = CurrentOwnerId
            };

            _db.Pets.Add(pet);
            await _db.SaveChangesAsync();

            if (dto.Photos != null && dto.Photos.Any())
            {
                if (dto.Photos.Count > 4)
                    return BadRequest("Можно добавить не более 4 фотографий.");

                await SavePhotos(pet.Id, dto.Photos);
                await _db.SaveChangesAsync();
            }

            return Ok(pet.Id);
        }

        // PUT /api/v2/pets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, [FromBody] PetUpdateDto dto)
        {
            var pet = await _db.Pets.FindAsync(id);
            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            if (!string.IsNullOrWhiteSpace(dto.Name)) pet.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Breed)) pet.Breed = dto.Breed;
            if (dto.WeightKg.HasValue) pet.WeightKg = dto.WeightKg;
            if (dto.BirthDate.HasValue) pet.BirthDate = dto.BirthDate;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Информация о питомце обновлена успешно." });
        }

        // PUT /api/v2/pets/{petId}/photos
        [HttpPut("{petId}/photos")]
        public async Task<IActionResult> UpdatePetPhotos(
            int petId,
            [FromForm] List<IFormFile>? newFiles,
            [FromForm] List<int>? deletePhotoIds)
        {
            var pet = await _db.Pets.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            if (deletePhotoIds != null && deletePhotoIds.Any())
            {
                var photosToDelete = pet.Photos.Where(p => deletePhotoIds.Contains(p.Id)).ToList();
                foreach (var photo in photosToDelete)
                {
                    DeletePhotoFile(photo.Url);
                    _db.PetPhotos.Remove(photo);
                }
            }

            if (newFiles != null && newFiles.Any())
            {
                int currentCount = pet.Photos.Count - (deletePhotoIds?.Count ?? 0);
                int availableSlots = 4 - currentCount;

                if (newFiles.Count > availableSlots)
                    return BadRequest($"Можно добавить максимум {availableSlots} новых фото (лимит — 4).");

                await SavePhotos(petId, newFiles);
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Фотографии питомца успешно обновлены." });
        }

        // POST /api/v2/pets/{petId}/upload
        [HttpPost("{petId}/upload")]
        public async Task<IActionResult> UploadPhoto(
            int petId,
            IFormFile file,
            [FromQuery] string? telegramFileId = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не найден.");

            var pet = await _db.Pets.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            if (pet.Photos.Count >= 4)
                return BadRequest("Превышен лимит: у питомца может быть не более 4 фотографий.");

            var relativeUrl = await SaveSinglePhoto(petId, file);

            var petPhoto = new PetPhoto
            {
                Url = relativeUrl,
                TelegramFileId = telegramFileId,
                PetId = petId
            };

            _db.PetPhotos.Add(petPhoto);
            await _db.SaveChangesAsync();

            return Ok(new { photoUrl = relativeUrl, petPhoto.Id });
        }

        // DELETE /api/v2/pets/{petId}/photos/{photoId}
        [HttpDelete("{petId}/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int petId, int photoId)
        {
            var pet = await _db.Pets.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            var photo = pet.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo is null)
                return NotFound();

            DeletePhotoFile(photo.Url);
            _db.PetPhotos.Remove(photo);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Фотография успешно удалена." });
        }

        /// <summary>Удалить питомца</summary>
        /// <remarks>Каскадно удаляет все события и фотографии питомца.</remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var pet = await _db.Pets.Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet is null)
                return NotFound();

            if (pet.OwnerId != CurrentOwnerId)
                return Forbid();

            foreach (var photo in pet.Photos)
                DeletePhotoFile(photo.Url);

            TryDeletePetFolder(id);

            _db.Pets.Remove(pet);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Питомец успешно удалён." });
        }

        private static PetDto MapToDto(Pet pet) => new()
        {
            Id = pet.Id,
            Name = pet.Name,
            Breed = pet.Breed,
            WeightKg = pet.WeightKg,
            BirthDate = pet.BirthDate,
            OwnerId = pet.OwnerId,
            Photos = pet.Photos.Select(ph => new PetPhotoDto
            {
                Id = ph.Id,
                Url = ph.Url,
                TelegramFileId = ph.TelegramFileId
            }).ToList()
        };

        private async Task SavePhotos(int petId, List<IFormFile> files)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", petId.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _db.PetPhotos.Add(new PetPhoto
                {
                    Url = $"/uploads/pets/{petId}/{fileName}",
                    PetId = petId
                });
            }
        }

        private async Task<string> SaveSinglePhoto(int petId, IFormFile file)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", petId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/pets/{petId}/{fileName}";
        }

        private void DeletePhotoFile(string relativeUrl)
        {
            var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", relativeUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); } catch { }
            }
        }

        private void TryDeletePetFolder(int petId)
        {
            try
            {
                var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", petId.ToString());
                if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
                    Directory.Delete(folder);
            }
            catch { }
        }
    }

    public class PetCreateV2Dto
    {
        public string Name { get; set; } = null!;
        public string? Breed { get; set; }
        public decimal? WeightKg { get; set; }
        public DateOnly? BirthDate { get; set; }
        public List<IFormFile>? Photos { get; set; }
    }
}
