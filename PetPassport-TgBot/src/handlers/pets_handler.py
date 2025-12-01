import os

from aiogram.fsm.context import FSMContext
from aiogram.types import CallbackQuery, FSInputFile
from aiogram import Router, F, types

from src.keyboard.keyboard import get_pets_list_keyboard, get_my_pet_keyboard
from src.services.photo_pet_service import download_pet_photo
from src.utils.api_client import get_pet_info
from src.utils.search_data import get_pet_data

router = Router()

@router.callback_query(lambda c: c.data == "pets_list")
async def pets_list(callback_query: CallbackQuery, state = FSMContext):
    tg_id = callback_query.from_user.id
    pets = await get_pet_data(tg_id)

    if not pets:
        await callback_query.answer("У вас пока нет питомцев!")
        return

    elif len(pets) == 1:
        pet = pets[0]
        pet_id = pet["id"]
        await callback_query.message.answer_photo(
            photo=FSInputFile("src/img/zaglushka.jpg"),
            caption=f"🐾 Питомец: {pet['name']}\nПорода: {pet.get('breed', 'не указана')}",
            parse_mode="Markdown",
            reply_markup=await get_my_pet_keyboard(pet_id),
        )
        await callback_query.answer()
        return

    elif len(pets) > 1:
        await callback_query.message.answer_photo(
            photo=FSInputFile("src/img/zaglushka.jpg"),
            caption="📋 Выбери питомца:",
            parse_mode="Markdown",
            reply_markup=await get_pets_list_keyboard(pets),
        )
        await callback_query.answer()


@router.callback_query(F.data.startswith("pet_"))
async def handle_pet_callback(callback_query: types.CallbackQuery):
    await callback_query.answer()
    pet_id = int(callback_query.data.split("_")[1])

    pet = await get_pet_info(pet_id)

    if not pet:
        await callback_query.answer("❌ Питомец не найден!", show_alert=True)
        return

    print(f"DEBUG: Complete pet data: {pet}")

    name = pet.get('name', 'Без имени')
    breed = pet.get('breed', 'не указана')
    weight = pet.get('weightKg', 'не указан')
    birth_date = pet.get('birthDate', 'не указана')
    photos = pet.get('photos', [])
    photo_count = len(photos)

    caption_lines = [
        f"🐾 *{name}*",
        f"📝 *Порода:* {breed}",
        f"⚖️ *Вес:* {weight} кг",
        f"🎂 *Дата рождения:* {birth_date}",
    ]

    caption = "\n".join(caption_lines)

    photo_sent = False

    if photos and len(photos) > 0:
        first_photo = photos[0]
        photo_url = first_photo.get('url')

        if photo_url and photo_url != "string":
            # Скачиваем фото
            local_photo_path = await download_pet_photo(pet_id, photo_url)

            if local_photo_path and os.path.exists(local_photo_path):
                try:
                    await callback_query.message.answer_photo(
                        photo=FSInputFile(local_photo_path),
                        caption=caption,
                        parse_mode="Markdown",
                        reply_markup=await get_my_pet_keyboard(pet_id),
                    )
                    photo_sent = True
                    print("✅ Фото питомца успешно отправлено")

                except Exception as e:
                    print(f"❌ Ошибка отправки фото: {e}")
                    photo_sent = False

    if not photo_sent:
        await callback_query.message.answer_photo(
            photo=FSInputFile("src/img/zaglushka.jpg"),
            caption=caption,
            parse_mode="Markdown",
            reply_markup=await get_my_pet_keyboard(pet_id),
        )
        print("⚠️ Использована заглушка")





