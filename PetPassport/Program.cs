using Microsoft.EntityFrameworkCore;
using PetPassport.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Подключаем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Настраиваем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // адрес фронтенда
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 3️⃣ Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4️⃣ Автоматически применяем миграции
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// 5️⃣ Настройка конвейера
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
