using System.ComponentModel.DataAnnotations;

namespace PetPassport.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        // Идентификация
        public PetSpecies? Species { get; set; }
        public PetGender? Gender { get; set; }

        [MaxLength(100)]
        public string? Breed { get; set; }

        [MaxLength(100)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? MicrochipNumber { get; set; }

        // Основные параметры
        public decimal? WeightKg { get; set; }
        public DateOnly? BirthDate { get; set; }

        // Медицинские данные
        public bool? IsNeutered { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? BloodType { get; set; }

        // FK к Owner
        public int OwnerId { get; set; }
        public Owner Owner { get; set; } = null!;

        public ICollection<PetPhoto> Photos { get; set; } = new List<PetPhoto>();
        public ICollection<PetEvent> Events { get; set; } = new List<PetEvent>();
    }
}
