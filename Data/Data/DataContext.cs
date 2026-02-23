using Data.Data.CMS;
using Data.Data.Magazyn;
using Microsoft.EntityFrameworkCore;

namespace Data.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Aktualnosc> Aktualnosc { get; set; } = default!;
        public DbSet<Strona> Strona { get; set; } = default!;
        public DbSet<Kategoria> Kategoria { get; set; } = default!;
        public DbSet<Produkt> Produkt { get; set; } = default!;
        public DbSet<JednostkaMiary> JednostkaMiary { get; set; } = default!;
        public DbSet<Lokacja> Lokacja { get; set; } = default!;
        public DbSet<Magazyn.Magazyn> Magazyn { get; set; } = default!;
        public DbSet<Rola> Rola { get; set; } = default!;
        public DbSet<RuchMagazynowy> RuchMagazynowy { get; set; } = default!;
        public DbSet<StanMagazynowy> StanMagazynowy { get; set; } = default!;
        public DbSet<Uzytkownik> Uzytkownik { get; set; } = default!;
        public DbSet<UzytkownikRola> UzytkownikRola { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Kategoria self-FK (u Ciebie)
            modelBuilder.Entity<Kategoria>()
                .HasOne(k => k.NadrzednaKategoria)
                .WithMany(k => k.Podkategorie)
                .HasForeignKey(k => k.IdNadrzednejKategorii)
                .OnDelete(DeleteBehavior.Restrict);

            // SKU unikalne
            modelBuilder.Entity<Produkt>()
                .HasIndex(p => p.Kod)
                .IsUnique();

            // Lokacja: kod unikalny w obrębie magazynu
            modelBuilder.Entity<Lokacja>()
                .HasIndex(l => new { l.IdMagazynu, l.Kod })
                .IsUnique();

            // Stock: 1 rekord na (produkt, lokacja)
            modelBuilder.Entity<StanMagazynowy>()
                .HasIndex(s => new { s.IdProduktu, s.IdLokacji })
                .IsUnique();

            // Users: login i email unikalne
            modelBuilder.Entity<Uzytkownik>().HasIndex(u => u.Login).IsUnique();
            modelBuilder.Entity<Uzytkownik>().HasIndex(u => u.Email).IsUnique();

            // UserRoles: unikalna para (User, Role)
            modelBuilder.Entity<UzytkownikRola>()
                .HasIndex(ur => new { ur.IdUzytkownika, ur.IdRoli })
                .IsUnique();
        }
    }
}
