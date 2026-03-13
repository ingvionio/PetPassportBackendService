namespace PetPassport.Models
{
    public class EventTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public EventType EventType { get; set; }

        // DoctorVisit
        public string? Clinic { get; set; }
        public string? Doctor { get; set; }
        public string? Diagnosis { get; set; }
        public string? Recommendations { get; set; }
        public string? Referrals { get; set; }

        // Vaccine
        public string? Medicine { get; set; }
        public int? PeriodValue { get; set; }
        public PeriodUnit? PeriodUnit { get; set; }

        // Treatment
        public string? Remedy { get; set; }
        public string? Parasite { get; set; }
        public int? TreatmentPeriodValue { get; set; }
        public PeriodUnit? TreatmentPeriodUnit { get; set; }
    }
}