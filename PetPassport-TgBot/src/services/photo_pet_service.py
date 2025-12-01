import glob
import time

from src.config import BASE_URL

import os
import aiohttp
from pathlib import Path

BASE_DIR = Path(__file__).parent.parent
TEMP_PHOTO_DIR = BASE_DIR / "temp_photo"
IMG_DIR = BASE_DIR / "src" / "img"

TEMP_PHOTO_DIR.mkdir(exist_ok=True)

async def download_pet_photo(pet_id: int, photo_url: str) -> str:
    try:
        if photo_url.startswith('/'):
            photo_url = photo_url[1:]
        full_photo_url = f"{BASE_URL}/{photo_url}"

        filename = f"pet_{pet_id}_{os.path.basename(photo_url)}"
        local_path = TEMP_PHOTO_DIR / filename

        print(f"DEBUG: Downloading from: {full_photo_url}")
        print(f"DEBUG: Saving to: {local_path}")
        print(f"DEBUG: Temp photo dir exists: {TEMP_PHOTO_DIR.exists()}")

        async with aiohttp.ClientSession() as session:
            async with session.get(full_photo_url, ssl=False) as response:
                if response.status == 200:

                    TEMP_PHOTO_DIR.mkdir(exist_ok=True)

                    with open(local_path, 'wb') as f:
                        f.write(await response.read())

                    print(f"✅ Фото успешно скачано: {local_path}")
                    print(f"DEBUG: File exists after download: {local_path.exists()}")
                    return str(local_path)
                else:
                    print(f"❌ Ошибка загрузки фото: {response.status}")
                    return None

    except Exception as e:
        print(f"❌ Ошибка при скачивании фото: {e}")
        return None


async def cleanup_old_photos(max_age_hours: int = 24):
    try:
        current_time = time.time()
        max_age_seconds = max_age_hours * 3600

        for file_path in glob.glob("temp_photo/pet_*.*"):
            if os.path.isfile(file_path):
                file_age = current_time - os.path.getmtime(file_path)
                if file_age > max_age_seconds:
                    os.remove(file_path)
                    print(f"🗑️ Удален старый файл: {file_path}")

    except Exception as e:
        print(f"❌ Ошибка при очистке фото: {e}")