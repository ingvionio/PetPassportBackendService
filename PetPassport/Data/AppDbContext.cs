// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using PetPassport.Models;

namespace PetPassport.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Owner> Owners { get; set; } = null!;
        public DbSet<Pet> Pets { get; set; } = null!;
        public DbSet<PetPhoto> PetPhotos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Owner.TelegramId уникальный
            modelBuilder.Entity<Owner>()
                .HasIndex(o => o.TelegramId)
                .IsUnique();

            // Owner -> Pets (1 - *), каскадное удаление
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pet -> Photos (1 - *)
            modelBuilder.Entity<PetPhoto>()
                .HasOne(pp => pp.Pet)
                .WithMany(p => p.Photos)
                .HasForeignKey(pp => pp.PetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Настройка колонок: пример для decimal precision
            modelBuilder.Entity<Pet>()
                .Property(p => p.WeightKg)
                .HasColumnType("numeric(6,2)"); // максимум 9999.99 кг, 2 знака после запятой
        }
    }

}   
