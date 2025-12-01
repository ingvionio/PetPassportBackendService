from aiogram import types, Router
from aiogram.fsm.context import FSMContext
from aiogram.filters import Command
from aiogram.types import FSInputFile

from src.utils.answer import GREETING_MESSAGE
from src.utils.api_client import register_owner, get_owner_by_telegram

from src.keyboard.keyboard import get_greeting_keyboard

router = Router()

@router.message(Command("start"))
async def auth_handler(message: types.Message, state: FSMContext):
    await state.clear()

    user_name = message.from_user.first_name
    tg_id = message.from_user.id
    tg_name = message.from_user.username

    owner_id = await get_owner_by_telegram(tg_id)
    if not owner_id:
        owner_id = await register_owner(tg_id, tg_name)
    await state.update_data(owner_id=owner_id, tg_id=tg_id)

    if not owner_id:
        await message.answer("Error")
        return

    else:
        await message.answer_photo(
            photo=FSInputFile("src/img/welcome.jpg"),
            caption=GREETING_MESSAGE.format(user_name=user_name),
            parse_mode="Markdown",
            reply_markup=await get_greeting_keyboard(),
        )