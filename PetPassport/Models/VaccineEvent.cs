namespace PetPassport.Models
{
    public class VaccineEvent : PetEvent
    {
        public string Medicine { get; set; } = null!;

        public int? PeriodValue { get; set; }
        public PeriodUnit? PeriodUnit { get; set; }

        public DateTime? NextVaccinationDate { get; set; }

        public VaccineEvent()
        {
            EventType = "Vaccine";
        }
    }

    public enum PeriodUnit
    {
        День,
        Месяц,
        Год
    }
}
