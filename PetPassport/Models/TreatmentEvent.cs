using PetPassport.Models;

public class TreatmentEvent : PetEvent
{
    public string Remedy { get; set; } = null!;      // средство
    public string Parasite { get; set; } = null!;    // паразит (клещи, глисты, блохи и т. п.)

    public int? PeriodValue { get; set; }            // например, каждые 3 месяца
    public PeriodUnit? PeriodUnit { get; set; }          // days / months / years

    public DateTime? NextTreatmentDate { get; set; } // дата следующей обработки

    public TreatmentEvent()
    {
        EventType = "Treatment";
    }
}
