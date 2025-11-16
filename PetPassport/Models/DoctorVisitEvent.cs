namespace PetPassport.Models
{
    public class DoctorVisitEvent : PetEvent
    {
        public string Clinic { get; set; }
        public string Doctor { get; set; }
        public string? Diagnosis { get; set; }
        public string? Recommendations { get; set; }
        public string? Referrals { get; set; }

        public DoctorVisitEvent()
        {
            EventType = "DoctorVisit";
        }
    }
}
