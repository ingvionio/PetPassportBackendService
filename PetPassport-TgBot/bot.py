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

async def on_startup(bot: Bot) -> None:
    await bot.set_webhook(f"{WEB_HOOK_URL}{WEB_HOOK_PATH}")

async def main():
    bot = Bot(token=BOT_TOKEN)
    dp = Dispatcher()

    dp.include_router(auth_handler.router)
    dp.include_router(add_pet_handler.router)
    dp.include_router(pets_handler.router)
    dp.include_router(settings_my_pet_handler.router)
    dp.startup.register(on_startup)

    app = web.Application()
    webhook_requests_handler = SimpleRequestHandler(
        dispatcher=dp,
        bot=bot,
    )

    webhook_requests_handler.register(app, path=WEB_HOOK_PATH)
    setup_application(app, dp, bot=bot)
    await web._run_app(app, host=WEB_SERVER_HOST)


if __name__ == "__main__":
    asyncio.run(main())
