from aiogram import Router, F
from aiogram.fsm.context import FSMContext
from aiogram.types import CallbackQuery, Message, FSInputFile

from src.keyboard.keyboard import get_settings_pet_keyboard
from src.states.update_pet_info_states import EditPetStates
from src.utils.api_client import update_pet, update_pet_photo

router = Router()

pending_edits: dict[int, dict] = {}

@router.callback_query(lambda c: c.data.startswith("settings_my_pet_"))
async def settings_pet_handler(callback_query: CallbackQuery, state: FSMContext):
    pet_id = int(callback_query.data.split("_")[-1])
    await state.update_data(pet_id=pet_id)

    await callback_query.message.answer_photo(
        photo=FSInputFile("src/img/zaglushka.jpg"),
        caption="📋 Выбери, что хочешь изменить:",
        parse_mode="Markdown",
        reply_markup=await get_settings_pet_keyboard(pet_id)
    )
    await state.set_state(EditPetStates.choosing_field)
    await callback_query.answer()


@router.callback_query(lambda c: c.data.startswith("edit_field_"))
async def start_edit_field(callback_query: CallbackQuery):
    parts = callback_query.data.split("_")

    if len(parts) < 4:
        print(f"DEBUG: Not enough parts: {len(parts)}")
        await callback_query.answer("❌ Ошибка в данных", show_alert=True)
        return

    field = parts[2]
    pet_id_str = parts[3]

    valid_fields = ["name", "breed", "weight", "birth", "photo"]
    if field not in valid_fields:
        print(f"DEBUG: Invalid field: {field}")
        await callback_query.answer("❌ Неизвестное поле", show_alert=True)
        return

    try:
        pet_id = int(pet_id_str)
        print(f"DEBUG: Field: {field}, Pet ID: {pet_id}")
    except ValueError:
        print(f"DEBUG: Cannot convert to int: {pet_id_str}")
        await callback_query.answer("❌ Ошибка: некорректный ID питомца", show_alert=True)
        return

    prompts = {
        "name": "✏️ Введи новое имя питомца:",
        "breed": "🐾 Введи новую породу питомца:",
        "weight": "⚖️ Введи новый вес питомца:",
        "birth": "🎂 Введи новую дату рождения (YYYY-MM-DD):",
        "photo": "🖼️ Отправь новое фото питомца:"
    }

    await callback_query.message.answer(
        prompts.get(field, "Введи новое значение:")
    )

    pending_edits[callback_query.from_user.id] = {"pet_id": pet_id, "field": field}
    await callback_query.answer()


@router.message(F.photo)
async def process_photo_update(message: Message):
    user_id = message.from_user.id

    if user_id not in pending_edits:
        return

    edit_data = pending_edits[user_id]

    # Проверяем, что редактируется именно фото
    if edit_data["field"] != "photo":
        return

    pet_id = edit_data["pet_id"]

    # Получаем фото с наилучшим качеством
    photo = message.photo[-1]
    file_id = photo.file_id

    # Получаем информацию о файле
    bot = message.bot
    file_info = await bot.get_file(file_id)
    file_path = file_info.file_path

    # Скачиваем файл
    file_bytes = await bot.download_file(file_path)

    # Отправляем фото на бекенд
    success = await update_pet_photo(pet_id, file_bytes.getvalue())

    if success:
        await message.answer("✅ Фото питомца успешно обновлено!")
    else:
        await message.answer("❌ Ошибка при обновлении фото.")

    del pending_edits[user_id]


@router.message()
async def process_field_update(message: Message):
    user_id = message.from_user.id

    edit_data = pending_edits[user_id]
    pet_id = edit_data["pet_id"]
    field = edit_data["field"]
    value = message.text.strip()

    kwargs = {}
    if field == "name":
        kwargs["name"] = value
    elif field == "breed":
        kwargs["breed"] = value
    elif field == "weight":
        try:
            kwargs["weight_kg"] = float(value.replace(",", "."))
        except ValueError:
            await message.answer("⚠️ Вес должен быть числом (например 4.5).")
            return
    elif field == "birth":
        from datetime import datetime
        try:
            datetime.strptime(value, "%Y-%m-%d")
            kwargs["birth_date"] = value
        except ValueError:
            await message.answer("⚠️ Формат даты должен быть YYYY-MM-DD.")
            return

    success = await update_pet(pet_id, **kwargs)
    if success:
        await message.answer("✅ Информация успешно обновлена!")
    else:
        await message.answer("❌ Ошибка при обновлении.")

    del pending_edits[user_id]
