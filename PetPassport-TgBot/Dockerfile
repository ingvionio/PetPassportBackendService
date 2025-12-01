FROM python:3.11-slim

WORKDIR /src

ENV PYTHONPATH=/src
# Устанавливаем только системные зависимости, не требующие компиляции
RUN apt-get update && apt-get install -y --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# Обновляем pip и устанавливаем setuptools
RUN pip install --upgrade pip setuptools wheel

# Копируем зависимости
COPY requirements.txt .

# Устанавливаем с использованием только wheels
RUN pip install --no-cache-dir -r requirements.txt

# Если всё равно ошибка, попробуйте установить отдельно
# RUN pip install --no-cache-dir "aiogram==3.10.0" --only-binary :all:
# RUN pip install --no-cache-dir "aiohttp==3.9.7" --only-binary :all:
# RUN pip install --no-cache-dir "python-dotenv==1.0.0"

COPY . .
RUN mkdir -p img

EXPOSE 8080

CMD ["python", "bot.py"]