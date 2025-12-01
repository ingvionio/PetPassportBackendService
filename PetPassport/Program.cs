using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Подключаем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Настраиваем CORS
var corsOriginsString = builder.Configuration["CORS_ORIGINS"] ?? "";
string[] corsOrigins = corsOriginsString
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Where(o => !string.IsNullOrWhiteSpace(o))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (corsOrigins.Length > 0)
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        else
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

// 3️⃣ Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4️⃣ Регистрируем сервисы для работы с ботом
builder.Services.AddHttpClient<IBotNotificationService, BotNotificationService>(client =>
{
    var baseUrl = builder.Configuration["BotService:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 5️⃣ Регистрируем Background Service для проверки напоминаний
builder.Services.AddHostedService<ReminderBackgroundService>();

var app = builder.Build();

// 6️⃣ Автоматически применяем миграции
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 7️⃣ Настройка конвейера
// Swagger доступен в Development и если явно включен через переменную окружения
var enableSwagger = app.Environment.IsDevelopment() || 
                     builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ CORS должно идти **до** MapControllers
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.Run();
