async def emoji_type(emoji_type):
    if emoji_type == "Посещение врача": return "👨‍⚕️"
    elif emoji_type == "Прививка": return "💉"
    elif emoji_type == "Обработка": return "💊"


async def emoji_pet(breed):
    breed_lower = breed.lower() if breed else ""

    # Собаки
    if any(word in breed_lower for word in ['лабрадор', 'овчарка', 'такса', 'терьер',
                                            'шпиц', 'пудель', 'бигль', 'дог', 'хаски']):
        return "🐕"
    elif 'чихуахуа' in breed_lower:
        return "🐶"
    elif 'корги' in breed_lower:
        return "🐕"

    # Кошки
    elif any(word in breed_lower for word in ['сфинкс', 'британ', 'шотланд', 'мейн-кун',
                                              'сиамск', 'перс', 'сибирск', 'беспород']):
        return "🐈"
    elif 'сфинкс' in breed_lower:
        return "🐱"

    # Другие животные
    elif any(word in breed_lower for word in ['кролик', 'кролик']):
        return "🐇"
    elif 'хомяк' in breed_lower:
        return "🐹"
    elif 'попугай' in breed_lower:
        return "🦜"
    elif 'крыса' in breed_lower:
        return "🐀"
    elif 'морская свинка' in breed_lower:
        return "🐹"
    elif 'шиншилла' in breed_lower:
        return "🐭"
    elif 'черепаха' in breed_lower:
        return "🐢"
    elif 'рыба' in breed_lower:
        return "🐠"
    elif 'попугай' in breed_lower:
        return "🦜"
    elif 'канарейка' in breed_lower:
        return "🐦"

    # По умолчанию - собака
    else:
        return "🐕"
