from src.utils.api_client import (
    get_owner_by_telegram,
    get_owner_pets,
)

async def get_pet_data(user_id: int, pet_id: int = None):
    owner_data = await get_owner_by_telegram(user_id)
    if not owner_data:
        return None

    owner_id = owner_data['ownerId']
    pets = await get_owner_pets(owner_id)

    if not pets:
        return None

    if pet_id is not None:
        for pet in pets:
            if pet["id"] == pet_id:
                return pet
        return None

    return pets