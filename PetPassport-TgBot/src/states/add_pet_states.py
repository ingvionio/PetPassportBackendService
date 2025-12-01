from aiogram.fsm.state import State,StatesGroup

class AddPetStates(StatesGroup):
    waiting_for_name = State()
    waiting_for_type = State()