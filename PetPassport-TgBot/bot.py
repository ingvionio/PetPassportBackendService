import asyncio

from aiogram import Bot, Dispatcher
from aiohttp import web
from aiogram.webhook.aiohttp_server import (
    SimpleRequestHandler,
    setup_application,
)

from src.config import (
    BOT_TOKEN,
    WEB_HOOK_URL,
    WEB_HOOK_PATH,
    WEB_SERVER_HOST
)

from src.handlers import (
    auth_handler,
    add_pet_handler,
    pets_handler,
    settings_my_pet_handler,
)
import logging
logging.basicConfig(level=logging.INFO)


async def on_startup(bot: Bot) -> None:
    await bot.set_webhook(f"{WEB_HOOK_URL}{WEB_HOOK_PATH}")


# -------------------------
# Новый обработчик /message
# -------------------------
async def message_handler(request: web.Request):
    try:
        data = await request.json()
        print("📩 Получено сообщение от ASP.NET:", data)

        telegram_id = data.get("telegramId") or data.get("telegram_id")
        pet_name = data.get("petName") or data.get("pet_name")
        event_title = data.get("eventTitle") or data.get("event_title")
        event_type = data.get("eventType") or data.get("event_type")
        event_date = data.get("eventDate") or data.get("event_date")

        if not telegram_id:
            return web.json_response({"error": "telegram_id is required"}, status=400)

        bot: Bot = request.app['bot']

        text = (
            f"🐾 Напоминание:\n\n"
            f"🐶 Питомец: {pet_name}\n"
            f"📌 Событие: {event_title}\n"
            f"📂 Тип: {event_type}\n"
            f"📅 Дата: {event_date}"
        )

        await bot.send_message(chat_id=telegram_id, text=text)

        return web.json_response({"status": "ok"})

    except Exception as e:
        print("❌ Ошибка обработки сообщения:", e)
        return web.json_response({"error": str(e)}, status=500)

async def main():
    bot = Bot(token=BOT_TOKEN)
    dp = Dispatcher()

    dp.include_router(auth_handler.router)
    dp.include_router(add_pet_handler.router)
    dp.include_router(pets_handler.router)
    dp.include_router(settings_my_pet_handler.router)
    dp.startup.register(on_startup)

    app = web.Application()

    # aiogram webhook обработчик
    webhook_handler = SimpleRequestHandler(dispatcher=dp, bot=bot)
    webhook_handler.register(app, path=WEB_HOOK_PATH)

    # -------------------------
    # Регистрируем новый маршрут
    # -------------------------
    app['bot'] = bot
    app.router.add_post("/message", message_handler)

    setup_application(app, dp, bot=bot)

    print("Webhook registered at:", WEB_HOOK_PATH)
    await web._run_app(app, host=WEB_SERVER_HOST, port=8080)


if __name__ == "__main__":
    asyncio.run(main())
