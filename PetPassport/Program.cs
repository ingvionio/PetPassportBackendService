using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PetPassport.Auth;
using PetPassport.Data;
using PetPassport.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2️⃣ CORS
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

// 3️⃣ JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// 4️⃣ Auth services
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<TelegramAuthService>();

// 5️⃣ Controllers + Swagger
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PetPassport API — v1 (legacy)",
        Version = "v1",
        Description = "Текущий API без авторизации. Используется существующим фронтендом и Telegram-ботом. " +
                      "Не рекомендуется для новых клиентов."
    });

    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "PetPassport API — v2",
        Version = "v2",
        Description = "Новая версия API с JWT-аутентификацией. " +
                      "Все эндпоинты кроме /auth требуют заголовок Authorization: Bearer {token}."
    });

    // Разделение v1 и v2 по URL-префиксу
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var isV2 = apiDesc.RelativePath?.StartsWith("api/v2") ?? false;
        return docName == "v2" ? isV2 : !isV2;
    });

    // Bearer token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите access token из ответа /api/v2/auth/login или /api/v2/auth/telegram"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // XML-комментарии из кода
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // Enums: в Swagger показываем "0 = Indefinite, 1 = Upcoming..." — API по-прежнему принимает числа
    c.SchemaFilter<PetPassport.Swagger.EnumSchemaFilter>();
});

// 6️⃣ Email service
builder.Services.AddTransient<IEmailService, EmailService>();

// 7️⃣ Bot notification service
builder.Services.AddHttpClient<IBotNotificationService, BotNotificationService>(client =>
{
    var baseUrl = builder.Configuration["BotService:BaseUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 9️⃣ Background reminder service
builder.Services.AddHostedService<ReminderBackgroundService>();

var app = builder.Build();

// 8️⃣ Apply migrations + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await EventTemplateSeeder.SeedAsync(db);
}

// 9️⃣ Pipeline
var enableSwagger = app.Environment.IsDevelopment() ||
                    builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "PetPassport API v2 (с авторизацией)");
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetPassport API v1 (legacy)");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowReactApp");

app.UseAuthentication(); // ← должно быть перед UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
