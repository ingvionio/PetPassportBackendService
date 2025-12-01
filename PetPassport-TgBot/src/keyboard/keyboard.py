from aiogram.types import (
    InlineKeyboardButton,
    InlineKeyboardMarkup,
)

from src.config import SERVER_URL


async def get_greeting_keyboard() -> InlineKeyboardMarkup:
    return InlineKeyboardMarkup(inline_keyboard=
    [
        [InlineKeyboardButton(text="Добавить питомца 🐶", callback_data="add_pet")],
        [InlineKeyboardButton(text="Посмотреть питомца(-ев) 🐱", callback_data="pets_list")],
    ])


async def get_pets_list_keyboard(pets: list[dict]) -> InlineKeyboardMarkup:
    buttons = [
        [InlineKeyboardButton(text=pet["name"], callback_data=f"pet_{pet['id']}")]
        for pet in pets
    ]
    return InlineKeyboardMarkup(inline_keyboard=buttons)


async def get_my_pet_keyboard(pet_id : int) -> InlineKeyboardMarkup:
    return InlineKeyboardMarkup(inline_keyboard=
    [
        # Формируем ссылку на паспорт питомца.
        # Добавляем разделяющий слэш, чтобы порт и id не слипались (7205 -> 7205/3, а не 72053).
        [InlineKeyboardButton(
            text="Паспорт питомца 📕",
            url=f"{SERVER_URL.rstrip('/')}/?id={pet_id}"
        )],
        [InlineKeyboardButton(text="Редактировать информацию о питомце 🐶", callback_data=f"settings_my_pet_{pet_id}")],
        [InlineKeyboardButton(text="Посмотреть питомца(-ев) 🐱", callback_data="pets_list")],
    ])

async def get_settings_pet_keyboard(pet_id: int) -> InlineKeyboardMarkup:
    return InlineKeyboardMarkup(inline_keyboard=
    [
        [InlineKeyboardButton(text="🐾 Имя", callback_data=f"edit_field_name_{pet_id}")],
        [InlineKeyboardButton(text="🐶 Порода", callback_data=f"edit_field_breed_{pet_id}")],
        [InlineKeyboardButton(text="⚖️ Вес", callback_data=f"edit_field_weight_{pet_id}")],
        [InlineKeyboardButton(text="📅 Дата рождения", callback_data=f"edit_field_birth_{pet_id}")],
        [InlineKeyboardButton(text="🖼️ Фото", callback_data=f"edit_field_photo_{pet_id}")],
    ])
