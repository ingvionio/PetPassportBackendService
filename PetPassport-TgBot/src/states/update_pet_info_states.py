from aiogram.fsm.state import StatesGroup, State

class EditPetStates(StatesGroup):
    choosing_field = State()
    waiting_for_value = State()