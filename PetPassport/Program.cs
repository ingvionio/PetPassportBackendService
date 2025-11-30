using Microsoft.EntityFrameworkCore;
using PetPassport.Data;
using PetPassport.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Подключаем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000",
            "http://localhost:5173") // адрес фронтенда
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
if (app.Environment.IsDevelopment())
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
