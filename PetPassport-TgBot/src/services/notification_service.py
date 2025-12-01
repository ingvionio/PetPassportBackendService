# http_handlers.py
from aiohttp import web

import bot
from src.services.emoji_service import emoji_type


async def notification_owner_handler(request):
    try:
        data = await request.json()

        telegram_id = data.get('telegram_id')
        pet_name = data.get('pet_name')
        event_type = data.get('event_type')
        event_title = data.get('event_title')
        event_date = data.get('event_date')

        print(f"[LOG] Получен POST запрос:")
        print(f"   Telegram ID: {telegram_id}")
        print(f"   Pet name: {pet_name}")
        print(f"   event_type: {event_type}")
        print(f"   event_title: {event_title}")
        print(f"   event_date: {event_date}")

        if not telegram_id:
            return web.json_response({
                'status': 'error',
                'message': 'Отсутствует telegram_id '
            }, status=400)

        if bot:
            try:
                message = f"{emoji_type(event_type)} Уведомление\n"
                if pet_name:
                    message += (f" Для питомца: {pet_name} ожидается {event_title}\n"
                                f" Дата: {event_date}\n")


                await bot.send_message(
                    chat_id=telegram_id,
                    text= message
                )
                print(f"✅ Сообщение отправлено пользователю {telegram_id}")

                return web.json_response({
                    'status': 'success',
                    'sent_to': telegram_id,
                    'message': event_title
                })

            except Exception as e:
                error_msg = f"❌ Ошибка отправки: {e}"
                print(error_msg)
                return web.json_response({
                    'status': 'error',
                    'message': str(e)
                }, status=500)
        else:
            return web.json_response({
                'status': 'error',
                'message': 'Бот не инициализирован'
            }, status=500)

    except Exception as e:
        print(f"❌ Ошибка обработки: {e}")
        return web.json_response({
            'status': 'error',
            'message': str(e)
        }, status=400)