using System.ComponentModel.DataAnnotations;

namespace PetPassport.Models
{
    // Models/PetPhoto.cs (тот же)
    public class PetPhoto
    {
        public int Id { get; set; }
        [Required]
        public string Url { get; set; } = string.Empty;
        public string? TelegramFileId { get; set; }
        public int PetId { get; set; }
        public Pet Pet { get; set; } = null!;
    }
}
