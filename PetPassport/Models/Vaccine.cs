namespace PetPassport.Models
{
    // Models/Vaccine.cs
    public class Vaccine
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateOnly VacinationDate {  get; set; }

        public int? PeriodValue { get; set; } // например, 6
        public PeriodUnit? PeriodUnit { get; set; } // Months

        public DateOnly? NextVacinationDate { get; set; }

        public int PetId { get; set; }

        public Pet pet { get; set; } = null!;

    }

    public enum PeriodUnit
    {
        День,
        Месяц,
        Год
    }
}
