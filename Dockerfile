# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY PetPassport/PetPassport.csproj PetPassport/
RUN dotnet restore PetPassport/PetPassport.csproj

# Копируем весь код и собираем проект
COPY PetPassport/ PetPassport/
WORKDIR /src/PetPassport
RUN dotnet build -c Release -o /app/build

# Публикуем приложение
RUN dotnet publish -c Release -o /app/publish

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Создаем директорию для загрузок
RUN mkdir -p /app/wwwroot/uploads/pets

# Открываем порт
EXPOSE 8080
EXPOSE 8081

# Устанавливаем переменную окружения для порта
ENV ASPNETCORE_URLS=http://+:8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "PetPassport.dll"]

