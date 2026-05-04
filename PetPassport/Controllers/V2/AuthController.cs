using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPassport.Auth;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers.V2
{
    [ApiController]
    [Route("api/v2/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly TelegramAuthService _telegram;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, JwtService jwt, TelegramAuthService telegram, IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _telegram = telegram;
            _config = config;
        }

        /// <summary>Вход через Telegram Web App</summary>
        /// <remarks>Передаёт initData из window.Telegram.WebApp.initData. Создаёт аккаунт автоматически при первом входе.</remarks>
        [HttpPost("telegram")]
        public async Task<ActionResult<AuthResponse>> LoginTelegram([FromBody] TelegramAuthRequest request)
        {
            var userInfo = _telegram.Verify(request.InitData);
            if (userInfo is null)
                return Unauthorized("Невалидные данные Telegram");

            var owner = await _db.Owners
                .FirstOrDefaultAsync(o => o.TelegramId == userInfo.TelegramId);

            if (owner is null)
            {
                owner = new Owner
                {
                    TelegramId = userInfo.TelegramId,
                    TelegramNick = userInfo.Username
                };
                _db.Owners.Add(owner);
                await _db.SaveChangesAsync();
            }

            return Ok(await IssueTokens(owner));
        }

        /// <summary>Регистрация по логину и паролю</summary>
        /// <remarks>Пароль хэшируется через BCrypt. Возвращает токены сразу после регистрации.</remarks>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Логин и пароль обязательны");

            var exists = await _db.Owners.AnyAsync(o => o.login == request.Login);
            if (exists)
                return Conflict("Пользователь с таким логином уже существует");

            var owner = new Owner
            {
                login = request.Login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _db.Owners.Add(owner);
            await _db.SaveChangesAsync();

            return Ok(await IssueTokens(owner));
        }

        /// <summary>Вход по логину и паролю</summary>
        /// <remarks>Возвращает accessToken (живёт 60 мин) и refreshToken (живёт 30 дней).
        /// Автоматически мигрирует пароль из v1 (открытый текст) в BCrypt при первом входе через v2.</remarks>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var owner = await _db.Owners
                .FirstOrDefaultAsync(o => o.login == request.Login);

            if (owner is null)
                return Unauthorized("Неверный логин или пароль");

            if (owner.PasswordHash is not null)
            {
                // Новый путь: проверяем BCrypt-хэш
                if (!BCrypt.Net.BCrypt.Verify(request.Password, owner.PasswordHash))
                    return Unauthorized("Неверный логин или пароль");
            }
            else if (owner.password is not null)
            {
                // Старый путь (v1): пароль в открытом виде — проверяем и сразу мигрируем
                if (owner.password != request.Password)
                    return Unauthorized("Неверный логин или пароль");

                owner.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _db.SaveChangesAsync();
            }
            else
            {
                return Unauthorized("У этого аккаунта нет пароля (вход через Telegram)");
            }

            return Ok(await IssueTokens(owner));
        }

        /// <summary>Обновить access token</summary>
        /// <remarks>Передаёт refreshToken — получает новую пару токенов. Старый refreshToken инвалидируется. Использовать когда accessToken вернул 401.</remarks>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
        {
            var stored = await _db.RefreshTokens
                .Include(rt => rt.Owner)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized("Refresh token недействителен или истёк");

            stored.IsRevoked = true;

            var response = await IssueTokens(stored.Owner);
            await _db.SaveChangesAsync();

            return Ok(response);
        }

        /// <summary>Выход из системы</summary>
        /// <remarks>Инвалидирует refreshToken. После этого обновить сессию будет невозможно — потребуется повторный вход.</remarks>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (stored is not null)
            {
                stored.IsRevoked = true;
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }

        private async Task<AuthResponse> IssueTokens(Owner owner)
        {
            var accessToken = _jwt.GenerateAccessToken(owner);
            var refreshTokenValue = _jwt.GenerateRefreshToken();

            var expiryDays = _config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 30);

            _db.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshTokenValue,
                OwnerId = owner.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
            });

            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                OwnerId = owner.Id
            };
        }
    }
}
