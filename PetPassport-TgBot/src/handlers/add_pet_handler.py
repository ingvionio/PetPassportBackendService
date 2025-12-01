from aiogram import Router, types
from aiogram.fsm.context import FSMContext

from src.states.add_pet_states import AddPetStates
from src.utils.api_client import add_pet, get_owner_by_telegram

router = Router()

@router.callback_query(lambda c: c.data == "add_pet")
async def add_pet_handler(callback_query: types.CallbackQuery, state: FSMContext):
    tg_id = callback_query.from_user.id
    owner_id = await get_owner_by_telegram(tg_id)

    if not owner_id:
        await callback_query.message.answer("⚠️ Не удалось определить владельца. Попробуй /start")
        return

    await state.update_data(owner_id=owner_id)
    await callback_query.message.answer("🐕 Введите имя питомца:")
    await state.set_state(AddPetStates.waiting_for_name)

@router.message(AddPetStates.waiting_for_name)
async def process_name(message: types.Message, state: FSMContext):
    await state.update_data(name=message.text)
    await message.answer("✨ Укажи вид питомца (например: кошка, собака):")
    await state.set_state(AddPetStates.waiting_for_type)

@router.message(AddPetStates.waiting_for_type)
async def process_type(message: types.Message, state: FSMContext):
    data = await state.get_data()
    tg_id = message.from_user.id
    owner_data = await get_owner_by_telegram(tg_id)
    name = data["name"]
    breed_ = message.text

    if owner_data and 'ownerId' in owner_data:
        owner_id = owner_data['ownerId']
    else:
        owner_id = None

    result = await add_pet(owner_id, name, breed_)
    await state.clear()

    if result:
        await message.answer(f"✅ Питомец *{name}* успешно добавлен!", parse_mode="Markdown")
    else:
        await message.answer("❌ Ошибка при добавлении питомца.")
