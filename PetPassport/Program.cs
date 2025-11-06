using Microsoft.EntityFrameworkCore;
using PetPassport.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Подключаем DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ Добавляем контроллеры и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3️⃣ Автоматически применяем миграции при запуске (опционально)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // если БД не существует — создаст
}

// 4️⃣ Конфигурация HTTP-конвейера
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles(); // чтобы можно было отдавать файлы из wwwroot


app.Run();
