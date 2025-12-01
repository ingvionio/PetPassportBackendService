# Развертывание PetPassport через Docker

## Требования

- Docker (версия 20.10+)
- Docker Compose (версия 2.0+)

## Быстрый старт

### 1. Подготовка

Убедитесь, что вы находитесь в корневой директории проекта (где находится `docker-compose.yml`).

### 2. Настройка переменных окружения

Создайте файл `.env` в корне проекта (опционально, можно задать переменные напрямую):

```env
POSTGRES_PASSWORD=your_secure_password_here
BOT_SERVICE_URL=http://your-bot-service:8000
```

Или отредактируйте `docker-compose.yml` напрямую, заменив `${POSTGRES_PASSWORD:-your_secure_password_here}` на ваш пароль.

### 3. Запуск

```bash
# Сборка и запуск всех сервисов
docker-compose up -d

# Просмотр логов
docker-compose logs -f api

# Остановка
docker-compose down

# Остановка с удалением данных БД
docker-compose down -v
```

### 4. Проверка

- API доступен по адресу: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger` (если включен в Production)
- PostgreSQL доступен на `localhost:5432`

## Команды для работы

```bash
# Пересобрать образ после изменений в коде
docker-compose build api
docker-compose up -d api

# Просмотр логов конкретного сервиса
docker-compose logs -f postgres
docker-compose logs -f api

# Выполнить миграции вручную (если нужно)
docker-compose exec api dotnet ef database update

# Подключиться к PostgreSQL
docker-compose exec postgres psql -U postgres -d PetPassportDb

# Перезапустить сервис
docker-compose restart api
```

## Структура

```
.
├── Dockerfile              # Образ для ASP.NET Core приложения
├── docker-compose.yml      # Конфигурация для запуска всех сервисов
├── .dockerignore           # Файлы, исключаемые из образа
├── .env                    # Переменные окружения (создать вручную)
└── PetPassport/
    └── ...                 # Исходный код приложения
```

## Переменные окружения

### В docker-compose.yml можно задать:

- `POSTGRES_PASSWORD` - пароль для PostgreSQL
- `BOT_SERVICE_URL` - URL сервиса бота (например, `http://bot-service:8000`)
- `ASPNETCORE_ENVIRONMENT` - окружение (Production/Development)

### В appsettings.json используются значения по умолчанию, которые переопределяются переменными окружения:

- `ConnectionStrings__DefaultConnection` - строка подключения к БД
- `BotService__BaseUrl` - URL сервиса бота

## Обновление приложения

```bash
# 1. Остановить контейнеры
docker-compose down

# 2. Пересобрать образ
docker-compose build --no-cache api

# 3. Запустить заново
docker-compose up -d
```

## Резервное копирование БД

```bash
# Создать бэкап
docker-compose exec postgres pg_dump -U postgres PetPassportDb > backup.sql

# Восстановить из бэкапа
docker-compose exec -T postgres psql -U postgres PetPassportDb < backup.sql
```

## Проблемы и решения

### Порт уже занят

Если порт 8080 или 5432 занят, измените в `docker-compose.yml`:

```yaml
ports:
  - "8081:8080"  # Внешний:Внутренний
```

### Миграции не применяются

Миграции применяются автоматически при запуске. Если нужно применить вручную:

```bash
docker-compose exec api dotnet ef database update
```

### Файлы загрузок не сохраняются

Убедитесь, что директория `./uploads` существует и имеет права на запись:

```bash
mkdir -p uploads
chmod 755 uploads
```

## Production развертывание

Для production рекомендуется:

1. Использовать секреты Docker или внешний менеджер секретов
2. Настроить HTTPS (через reverse proxy, например nginx)
3. Использовать внешнюю БД (не в контейнере)
4. Настроить мониторинг и логирование
5. Использовать health checks

Пример с nginx reverse proxy можно добавить в `docker-compose.yml`:

```yaml
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api
```

