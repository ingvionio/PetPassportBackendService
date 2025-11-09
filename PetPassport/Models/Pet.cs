using System.ComponentModel.DataAnnotations;

namespace PetPassport.Models
{
    // Models/Pet.cs
    public class Pet
    {
        public int Id { get; set; }                 // PK

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Breed { get; set; }

        public decimal? WeightKg { get; set; }      // numeric

        public DateOnly? BirthDate { get; set; }

        // FK к Owner
        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;

        public ICollection<PetPhoto> Photos { get; set; } = new List<PetPhoto>();

        public ICollection<Vaccine> Vaccines { get; set;} = new List<Vaccine>();
    }
}
