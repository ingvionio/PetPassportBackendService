using System.ComponentModel.DataAnnotations;

namespace PetPassport.Models
{
    // Models/Owner.cs
    public class Owner
    {
        public int Id { get; set; }                 // суррогатный PK (int serial)
        public long TelegramId { get; set; }        // внешний идентификатор (unique)
        [MaxLength(100)]
        public string? TelegramNick { get; set; }

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}
