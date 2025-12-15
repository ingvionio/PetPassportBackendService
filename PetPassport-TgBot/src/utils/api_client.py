from datetime import datetime

import aiohttp

from src.config import BASE_URL
from typing import (
    Optional,
    Dict,
    Any,
    List
)

async def register_owner(telegram_id: int, telegram_nick: Optional[str]) -> Optional[int]:
    payload = {
        "telegramId": telegram_id,
        "telegramNick": telegram_nick or "Unknown"
    }

    # Отключаем проверку SSL-сертификата для локального https (self-signed)
    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.post(f"{BASE_URL}/api/Owners/register", json=payload) as resp:
            print("Ответ сервера при регистрации владельца:", resp.status)
            if resp.status in (200, 201):
                data = await resp.json()
                print("Ответ JSON:", data)

                if isinstance(data, int):
                    return data
                return data.get("id")
            else:
                print("Ошибка регистрации:", resp.status, await resp.text())
                return None


async def get_owner_by_telegram(telegram_id: int) -> Optional[Dict[str, Any]]:
    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.get(f"{BASE_URL}/api/Owners/by-telegram/{telegram_id}") as resp:
            if resp.status == 200:
                return await resp.json()
            print("Владелец не найден:", resp.status)
            return None


async def get_owner_pets(owner_id: int) -> Optional[List[Dict[str, Any]]]:
    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.get(f"{BASE_URL}/api/Owners/{owner_id}/pets") as resp:
            if resp.status == 200:
                return await resp.json()
            print("Ошибка при получении питомцев:", resp.status)
            return None

async def add_pet(owner_id: int, name: str, breed: str):
    payload = {
        "name": name,
        "breed": breed,
        "ownerId": int(owner_id)
    }

    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.post(f"{BASE_URL}/api/Pets", json=payload) as resp:
            print(f"Ответ сервера: {resp.status}")
            if resp.status in (200, 201):
                return await resp.json()
            else:
                text = await resp.text()
                print("Ошибка при добавлении питомца:", text)
                return None

async def update_pet(pet_id: int,
                     name: str = None,
                     breed: str = None,
                     weight_kg: float = None,
                     birth_date: str = None
                     ) -> bool:
    payload = {}

    if name:
        payload["name"] = name
    if breed:
        payload["breed"] = breed
    if weight_kg:
        payload["weightKg"] = weight_kg
    if birth_date:
        payload["birthDate"] = birth_date

    if not payload:
        return False

    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.put(f"{BASE_URL}/api/Pets/{pet_id}", json=payload) as resp:
            if resp.status == 200:
                return True
            print("Ошибка при обновлении питомца:", resp.status)
            return False

async def get_pet_info(pet_id: int) -> Optional[Dict[str, Any]]:
    async with aiohttp.ClientSession(connector=aiohttp.TCPConnector(ssl=False)) as session:
        async with session.get(f"{BASE_URL}/api/Pets/{pet_id}") as resp:
            if resp.status == 200:
                data = await resp.json()
                print(f"DEBUG: Raw API response: {data}")

                normalized_data = {}

                for key, value in data.items():
                    if key == 'weight8':
                        normalized_data['weightKg'] = value
                    elif key == 'birthday':
                        normalized_data['birthDate'] = value
                    elif key == 'parent8':
                        normalized_data['ownerId'] = value
                    else:
                        normalized_data[key] = value

                if 'photos' not in normalized_data:
                    normalized_data['photos'] = []

                print(f"DEBUG: Normalized data: {normalized_data}")
                return normalized_data

            print(f"Ошибка при получении питомца {pet_id}:", resp.status)
            return None


async def update_pet_photo(pet_id: int, photo_bytes: bytes) -> bool:
    try:
        import aiohttp
        import io

        form_data = aiohttp.FormData()
        form_data.add_field('file',
                            io.BytesIO(photo_bytes),
                            filename=f'pet_{pet_id}.jpg',
                            content_type='image/jpeg')

        async with aiohttp.ClientSession() as session:
            async with session.post(
                    f"{BASE_URL}/api/Pets/{pet_id}/upload",
                    data=form_data,
                    ssl=False
            ) as response:
                if response.status == 200:
                    print("✅ Фото успешно загружено на бекенд")
                    return True
                else:
                    print(f"❌ Ошибка загрузки фото: {response.status}")
                    return False

    except Exception as e:
        print(f"❌ Исключение при загрузке фото: {e}")
        return False


async def send_message_to_bot(telegram_id:int, pet_name:str, event_title: str, event_type:str,event_date: str, custom_data: dict = None):

    payload = {
        "telegram_id": telegram_id,
        "pet_name": pet_name,
        "event_type": event_type,
        "event_title": event_title,
        "event_date": event_date
    }

    if custom_data:
        payload.update(custom_data)

    try:
        async with aiohttp.ClientSession() as session:
            async with session.post(
                    'http://localhost:8080/message',
                    json=payload,
                    headers={'Content-Type': 'application/json'}
            ) as resp:
                response_data = await resp.json()
                print(f"✅ Статус: {resp.status}")
                print(f"✅ Ответ: {response_data}")
                return response_data

    except Exception as e:
        print(f"❌ Ошибка: {e}")
        return None