using Microsoft.AspNetCore.Mvc;
using PetPassport.Data;
using PetPassport.Models;

namespace PetPassport.Controllers.V2
{
    [ApiController]
    [Route("api/v2/treatment")]
    public class TreatmentV2Controller : EventControllerBaseV2<TreatmentEvent, TreatmentDto>
    {
        public TreatmentV2Controller(AppDbContext db) : base(db) { }

        protected override TreatmentEvent MapCreateDto(TreatmentDto dto)
        {
            var entity = new TreatmentEvent
            {
                Remedy = dto.Remedy,
                Parasite = dto.Parasite,
                PeriodValue = dto.PeriodValue,
                PeriodUnit = dto.PeriodUnit,
                NextTreatmentDate = dto.NextTreatmentDate
            };
            MapBaseDto(entity, dto);
            return entity;
        }

        protected override void MapUpdateDto(TreatmentEvent entity, TreatmentDto dto)
        {
            if (dto.Title != null) entity.Title = dto.Title;
            if (dto.EventDate != default) entity.EventDate = dto.EventDate;
            if (dto.Remedy != null) entity.Remedy = dto.Remedy;
            if (dto.Parasite != null) entity.Parasite = dto.Parasite;
            if (dto.PeriodValue != null) entity.PeriodValue = dto.PeriodValue;
            if (dto.PeriodUnit != null) entity.PeriodUnit = dto.PeriodUnit;
            if (dto.NextTreatmentDate != null) entity.NextTreatmentDate = dto.NextTreatmentDate;
            if (dto.ReminderEnabled != null) entity.ReminderEnabled = dto.ReminderEnabled;
            if (dto.ReminderValue != null) entity.ReminderValue = dto.ReminderValue;
            if (dto.ReminderUnit != null) entity.ReminderUnit = dto.ReminderUnit;
        }

        protected override TreatmentDto MapToReturnDto(TreatmentEvent t)
        {
            var dto = new TreatmentDto
            {
                Remedy = t.Remedy,
                Parasite = t.Parasite,
                PeriodValue = t.PeriodValue,
                PeriodUnit = t.PeriodUnit,
                NextTreatmentDate = t.NextTreatmentDate
            };
            dto.MapFromEntity(t);
            return dto;
        }
    }
}
