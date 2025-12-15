# http_handlers.py
import logging

from aiogram import Bot
from aiohttp import web

from src.services.emoji_service import (
    emoji_type,
    emoji_pet
)


async def notification_owner_handler(request: web.Request):
    try:
        data = await request.json()

        # 1. Получение объекта Бота
        bot: Bot = request.app.get('bot')
        if not bot:
            logging.error("Бот не найден в контексте приложения.")
            return web.json_response({'status': 'error', 'message': 'Бот не инициализирован'}, status=500)

        # 2. Извлечение и валидация данных
        # Обрабатываем возможные варианты именования ключа ID
        t_id = data.get('telegram_id') or data.get('telegramId')

        if not t_id:
            logging.warning("Отсутствует telegram_id в запросе.")
            return web.json_response({'status': 'error', 'message': 'Отсутствует telegram_id'}, status=400)

        try:
            telegram_id = int(t_id)  # Приводим к int
        except ValueError:
            logging.warning(f"Неверный формат telegram_id: {t_id}")
            return web.json_response({'status': 'error', 'message': 'telegram_id должен быть числом'}, status=400)

        # 3. Извлечение остальных полей
        pet_name = data.get('pet_name') or data.get('petName', 'Не указан')
        event_type = data.get('event_type') or data.get('eventType', 'событие')
        event_title = data.get('event_title') or data.get('eventTitle', 'Напоминание')
        event_date = data.get('event_date') or data.get('eventDate', '-')
        pet_breed = data.get('pet_breed') or data.get('petBreed')

        # 4. Формирование сообщения

        message = f"{emoji_type(event_type)} Уведомление\n"
        message += (f"{emoji_pet(pet_breed)} Для питомца: {pet_name} ожидается {event_title}\n"
                    f"📅 Дата: {event_date}\n")

        logging.info(f"📩 POST запрос: ID={telegram_id}, Событие='{event_title}'")

        # 5. Отправка сообщения
        await bot.send_message(
            chat_id=telegram_id,
            text=message
        )

        logging.info(f"✅ Сообщение успешно отправлено пользователю {telegram_id}")

        return web.json_response({
            'status': 'success',
            'sent_to': telegram_id,
            'message': event_title
        })

    except Exception as e:
        # Ловим ошибки Telegram API (например, Forbidden: Bot blocked by user)
        logging.error(f"❌ Ошибка обработки или отправки сообщения: {e}")
        return web.json_response({
            'status': 'error',
            'message': str(e)
        }, status=500)