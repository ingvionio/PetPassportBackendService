using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Data;

namespace PetPassport.Controllers.V2
{
    [Authorize]
    [ApiController]
    [Route("api/v2/account")]
    public class AccountV2Controller : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AccountV2Controller(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private int CurrentOwnerId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Получить профиль текущего пользователя</summary>
        [HttpGet]
        public async Task<ActionResult<AccountDto>> GetAccount()
        {
            var owner = await _db.Owners
                .Include(o => o.Pets)
                .FirstOrDefaultAsync(o => o.Id == CurrentOwnerId);

            if (owner is null)
                return NotFound();

            return Ok(new AccountDto
            {
                Id = owner.Id,
                Login = owner.login,
                TelegramId = owner.TelegramId,
                TelegramNick = owner.TelegramNick,
                PetCount = owner.Pets.Count
            });
        }

        /// <summary>Список питомцев текущего пользователя (id + name)</summary>
        [HttpGet("pets")]
        public async Task<ActionResult<List<PetSummaryDto>>> GetMyPetsSummary()
        {
            var pets = await _db.Pets
                .Where(p => p.OwnerId == CurrentOwnerId)
                .Select(p => new PetSummaryDto { Id = p.Id, Name = p.Name })
                .ToListAsync();

            return Ok(pets);
        }

        /// <summary>Сменить пароль</summary>
        /// <remarks>Только для аккаунтов с логином/паролем. Telegram-аккаунты без пароля получат 400.</remarks>
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest("Новый пароль должен содержать минимум 6 символов");

            var owner = await _db.Owners.FindAsync(CurrentOwnerId);
            if (owner is null)
                return NotFound();

            if (owner.PasswordHash is null)
                return BadRequest("У этого аккаунта нет пароля (вход через Telegram)");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, owner.PasswordHash))
                return BadRequest("Неверный текущий пароль");

            owner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Пароль успешно изменён" });
        }

        /// <summary>Удалить аккаунт</summary>
        /// <remarks>Необратимо удаляет аккаунт и все связанные данные: питомцев, события, фотографии, refresh-токены. Файлы фото удаляются с диска.</remarks>
        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var owner = await _db.Owners
                .Include(o => o.Pets)
                    .ThenInclude(p => p.Photos)
                .FirstOrDefaultAsync(o => o.Id == CurrentOwnerId);

            if (owner is null)
                return NotFound();

            // Удаляем файлы фотографий с диска
            foreach (var pet in owner.Pets)
            {
                foreach (var photo in pet.Photos)
                {
                    var fullPath = Path.Combine(_env.WebRootPath ?? "wwwroot", photo.Url.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        try { System.IO.File.Delete(fullPath); } catch { }
                    }
                }

                // Удаляем папку питомца
                try
                {
                    var petFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "pets", pet.Id.ToString());
                    if (Directory.Exists(petFolder))
                        Directory.Delete(petFolder, recursive: true);
                }
                catch { }
            }

            // Удаляем владельца — питомцы, события, refresh-токены удалятся каскадно
            _db.Owners.Remove(owner);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Аккаунт и все связанные данные удалены" });
        }
    }

    public class AccountDto
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public long? TelegramId { get; set; }
        public string? TelegramNick { get; set; }
        public int PetCount { get; set; }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
