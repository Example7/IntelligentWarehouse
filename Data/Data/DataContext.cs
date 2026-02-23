using MagazynEntity = global::Data.Data.Magazyn.Magazyn;
using Data.Data.CMS;
using Data.Data.Magazyn;
using Microsoft.EntityFrameworkCore;

namespace Data.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Aktualnosc> Aktualnosc { get; set; } = default!;
        public DbSet<Strona> Strona { get; set; } = default!;

        public DbSet<PlikMedia> PlikMedia { get; set; } = default!;
        public DbSet<SzablonWydruku> SzablonWydruku { get; set; } = default!;
        public DbSet<ZalacznikDokumentu> ZalacznikDokumentu { get; set; } = default!;

        public DbSet<Kategoria> Kategoria { get; set; } = default!;
        public DbSet<Produkt> Produkt { get; set; } = default!;
        public DbSet<JednostkaMiary> JednostkaMiary { get; set; } = default!;
        public DbSet<MagazynEntity> Magazyn { get; set; } = default!;
        public DbSet<Lokacja> Lokacja { get; set; } = default!;
        public DbSet<StanMagazynowy> StanMagazynowy { get; set; } = default!;
        public DbSet<RuchMagazynowy> RuchMagazynowy { get; set; } = default!;
        public DbSet<Uzytkownik> Uzytkownik { get; set; } = default!;
        public DbSet<Rola> Rola { get; set; } = default!;
        public DbSet<UzytkownikRola> UzytkownikRola { get; set; } = default!;

        public DbSet<Dostawca> Dostawca { get; set; } = default!;
        public DbSet<Klient> Klient { get; set; } = default!;

        public DbSet<DokumentPZ> DokumentPZ { get; set; } = default!;
        public DbSet<PozycjaPZ> PozycjaPZ { get; set; } = default!;
        public DbSet<DokumentWZ> DokumentWZ { get; set; } = default!;
        public DbSet<PozycjaWZ> PozycjaWZ { get; set; } = default!;
        public DbSet<DokumentMM> DokumentMM { get; set; } = default!;
        public DbSet<PozycjaMM> PozycjaMM { get; set; } = default!;
        public DbSet<Inwentaryzacja> Inwentaryzacja { get; set; } = default!;
        public DbSet<PozycjaInwentaryzacji> PozycjaInwentaryzacji { get; set; } = default!;
        public DbSet<Rezerwacja> Rezerwacja { get; set; } = default!;
        public DbSet<PozycjaRezerwacji> PozycjaRezerwacji { get; set; } = default!;

        public DbSet<KodProduktu> KodProduktu { get; set; } = default!;
        public DbSet<ProduktJednostka> ProduktJednostka { get; set; } = default!;
        public DbSet<Partia> Partia { get; set; } = default!;

        public DbSet<LogAudytu> LogAudytu { get; set; } = default!;
        public DbSet<UstawienieAplikacji> UstawienieAplikacji { get; set; } = default!;
        public DbSet<RegulaAlertu> RegulaAlertu { get; set; } = default!;
        public DbSet<Alert> Alert { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Kategoria (self FK)
            modelBuilder.Entity<Kategoria>()
                .HasOne(k => k.NadrzednaKategoria)
                .WithMany(k => k.Podkategorie)
                .HasForeignKey(k => k.IdNadrzednejKategorii)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Kategoria>()
                .HasIndex(k => k.IdNadrzednejKategorii);

            // --- Produkt
            modelBuilder.Entity<Produkt>().HasIndex(p => p.Kod).IsUnique();
            modelBuilder.Entity<Produkt>().HasIndex(p => p.IdKategorii);
            modelBuilder.Entity<Produkt>().HasIndex(p => p.IdDomyslnejJednostki);

            modelBuilder.Entity<Produkt>()
                .HasOne(p => p.Kategoria)
                .WithMany(k => k.Produkty)
                .HasForeignKey(p => p.IdKategorii)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Produkt>()
                .HasOne(p => p.DomyslnaJednostka)
                .WithMany()
                .HasForeignKey(p => p.IdDomyslnejJednostki)
                .OnDelete(DeleteBehavior.Restrict);

            // --- JednostkaMiary / Rola
            modelBuilder.Entity<JednostkaMiary>().HasIndex(u => u.Kod).IsUnique();
            modelBuilder.Entity<Rola>().HasIndex(r => r.Nazwa).IsUnique();

            // --- Lokacja / Magazyn
            modelBuilder.Entity<Lokacja>()
                .HasIndex(l => new { l.IdMagazynu, l.Kod })
                .IsUnique();

            modelBuilder.Entity<Lokacja>()
                .HasOne(l => l.Magazyn)
                .WithMany(m => m.Lokacje)
                .HasForeignKey(l => l.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Stock
            modelBuilder.Entity<StanMagazynowy>()
                .HasIndex(s => new { s.IdProduktu, s.IdLokacji })
                .IsUnique();

            modelBuilder.Entity<StanMagazynowy>()
                .HasOne(s => s.Produkt)
                .WithMany()
                .HasForeignKey(s => s.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StanMagazynowy>()
                .HasOne(s => s.Lokacja)
                .WithMany(l => l.Stany)
                .HasForeignKey(s => s.IdLokacji)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Ruchy (indeksy pod raporty + restricty)
            modelBuilder.Entity<RuchMagazynowy>().HasIndex(r => r.UtworzonoUtc);
            modelBuilder.Entity<RuchMagazynowy>().HasIndex(r => r.IdProduktu);
            modelBuilder.Entity<RuchMagazynowy>().HasIndex(r => r.Typ);

            modelBuilder.Entity<RuchMagazynowy>()
                .HasOne(r => r.Produkt)
                .WithMany()
                .HasForeignKey(r => r.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RuchMagazynowy>()
                .HasOne(r => r.LokacjaZ)
                .WithMany()
                .HasForeignKey(r => r.IdLokacjiZ)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RuchMagazynowy>()
                .HasOne(r => r.LokacjaDo)
                .WithMany()
                .HasForeignKey(r => r.IdLokacjiDo)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Users
            modelBuilder.Entity<Uzytkownik>().HasIndex(u => u.Login).IsUnique();
            modelBuilder.Entity<Uzytkownik>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<UzytkownikRola>()
                .HasIndex(ur => new { ur.IdUzytkownika, ur.IdRoli })
                .IsUnique();

            modelBuilder.Entity<UzytkownikRola>()
                .HasOne(ur => ur.Uzytkownik)
                .WithMany(u => u.RoleUzytkownika)
                .HasForeignKey(ur => ur.IdUzytkownika)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UzytkownikRola>()
                .HasOne(ur => ur.Rola)
                .WithMany(r => r.Uzytkownicy)
                .HasForeignKey(ur => ur.IdRoli)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Dostawca / Klient
            modelBuilder.Entity<Dostawca>().HasIndex(d => d.Nazwa);
            modelBuilder.Entity<Dostawca>().HasIndex(d => d.NIP);

            modelBuilder.Entity<Klient>().HasIndex(k => k.Nazwa);
            modelBuilder.Entity<Klient>().HasIndex(k => k.Email);
            modelBuilder.Entity<Klient>().HasIndex(k => k.IdUzytkownika);

            modelBuilder.Entity<Klient>()
                .HasOne(k => k.Uzytkownik)
                .WithMany(u => u.Klienci)
                .HasForeignKey(k => k.IdUzytkownika)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Dokumenty: numery unikalne + (DocId, LineNo) unikalne
            modelBuilder.Entity<DokumentPZ>().HasIndex(d => d.Numer).IsUnique();
            modelBuilder.Entity<PozycjaPZ>().HasIndex(p => new { p.IdDokumentu, p.Lp }).IsUnique();

            modelBuilder.Entity<DokumentWZ>().HasIndex(d => d.Numer).IsUnique();
            modelBuilder.Entity<PozycjaWZ>().HasIndex(p => new { p.IdDokumentu, p.Lp }).IsUnique();

            modelBuilder.Entity<DokumentMM>().HasIndex(d => d.Numer).IsUnique();
            modelBuilder.Entity<PozycjaMM>().HasIndex(p => new { p.IdDokumentu, p.Lp }).IsUnique();

            modelBuilder.Entity<Inwentaryzacja>().HasIndex(d => d.Numer).IsUnique();
            modelBuilder.Entity<PozycjaInwentaryzacji>().HasIndex(p => new { p.IdDokumentu, p.Lp }).IsUnique();

            modelBuilder.Entity<Rezerwacja>().HasIndex(d => d.Numer).IsUnique();
            modelBuilder.Entity<PozycjaRezerwacji>().HasIndex(p => new { p.IdRezerwacji, p.Lp }).IsUnique();

            // Doc -> Items: cascade OK (usuwasz dokument, usuwają się pozycje)
            modelBuilder.Entity<PozycjaPZ>()
                .HasOne(p => p.Dokument)
                .WithMany(d => d.Pozycje)
                .HasForeignKey(p => p.IdDokumentu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PozycjaWZ>()
                .HasOne(p => p.Dokument)
                .WithMany(d => d.Pozycje)
                .HasForeignKey(p => p.IdDokumentu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PozycjaMM>()
                .HasOne(p => p.Dokument)
                .WithMany(d => d.Pozycje)
                .HasForeignKey(p => p.IdDokumentu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PozycjaInwentaryzacji>()
                .HasOne(p => p.Dokument)
                .WithMany(d => d.Pozycje)
                .HasForeignKey(p => p.IdDokumentu)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PozycjaRezerwacji>()
                .HasOne(p => p.Rezerwacja)
                .WithMany(r => r.Pozycje)
                .HasForeignKey(p => p.IdRezerwacji)
                .OnDelete(DeleteBehavior.Cascade);

            // --- ProductCodes / ProductUoms / Batches
            modelBuilder.Entity<KodProduktu>().HasIndex(x => x.Wartosc).IsUnique();
            modelBuilder.Entity<ProduktJednostka>().HasIndex(x => new { x.IdProduktu, x.IdJednostki }).IsUnique();
            modelBuilder.Entity<Partia>().HasIndex(x => new { x.IdProduktu, x.NumerPartii }).IsUnique();

            // --- CMS: media/templates/attachments (indeksy)
            modelBuilder.Entity<PlikMedia>().HasIndex(x => x.Sciezka).IsUnique();
            modelBuilder.Entity<SzablonWydruku>().HasIndex(x => new { x.TypDokumentu, x.Wersja }).IsUnique();
            modelBuilder.Entity<ZalacznikDokumentu>().HasIndex(x => new { x.TypDokumentu, x.IdDokumentu });

            // --- Audyt / ustawienia / alerty
            modelBuilder.Entity<LogAudytu>().HasIndex(x => x.KiedyUtc);
            modelBuilder.Entity<LogAudytu>().HasIndex(x => x.Encja);

            modelBuilder.Entity<RegulaAlertu>().HasIndex(x => new { x.IdMagazynu, x.IdProduktu, x.Typ });
            modelBuilder.Entity<Alert>().HasIndex(x => x.UtworzonoUtc);
            modelBuilder.Entity<Alert>().HasIndex(x => x.CzyPotwierdzony);

            // --- DokumentPZ: FK restricty
            modelBuilder.Entity<DokumentPZ>()
                .HasOne(d => d.Magazyn)
                .WithMany()
                .HasForeignKey(d => d.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DokumentPZ>()
                .HasOne(d => d.Dostawca)
                .WithMany(x => x.Przyjecia)
                .HasForeignKey(d => d.IdDostawcy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DokumentPZ>()
                .HasOne(d => d.Utworzyl)
                .WithMany()
                .HasForeignKey(d => d.IdUtworzyl)
                .OnDelete(DeleteBehavior.Restrict);

            // PozycjaPZ: produkt/lokacja restricty
            modelBuilder.Entity<PozycjaPZ>()
                .HasOne(p => p.Produkt)
                .WithMany()
                .HasForeignKey(p => p.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaPZ>()
                .HasOne(p => p.Lokacja)
                .WithMany()
                .HasForeignKey(p => p.IdLokacji)
                .OnDelete(DeleteBehavior.Restrict);


            // --- DokumentWZ: FK restricty
            modelBuilder.Entity<DokumentWZ>()
                .HasOne(d => d.Magazyn)
                .WithMany()
                .HasForeignKey(d => d.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DokumentWZ>()
                .HasOne(d => d.Klient)
                .WithMany(x => x.Wydania)
                .HasForeignKey(d => d.IdKlienta)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DokumentWZ>()
                .HasOne(d => d.Utworzyl)
                .WithMany()
                .HasForeignKey(d => d.IdUtworzyl)
                .OnDelete(DeleteBehavior.Restrict);

            // PozycjaWZ: produkt/lokacja restricty
            modelBuilder.Entity<PozycjaWZ>()
                .HasOne(p => p.Produkt)
                .WithMany()
                .HasForeignKey(p => p.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaWZ>()
                .HasOne(p => p.Lokacja)
                .WithMany()
                .HasForeignKey(p => p.IdLokacji)
                .OnDelete(DeleteBehavior.Restrict);


            // --- DokumentMM: FK restricty
            modelBuilder.Entity<DokumentMM>()
                .HasOne(d => d.Magazyn)
                .WithMany()
                .HasForeignKey(d => d.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DokumentMM>()
                .HasOne(d => d.Utworzyl)
                .WithMany()
                .HasForeignKey(d => d.IdUtworzyl)
                .OnDelete(DeleteBehavior.Restrict);

            // PozycjaMM: produkt/lokacje restricty
            modelBuilder.Entity<PozycjaMM>()
                .HasOne(p => p.Produkt)
                .WithMany()
                .HasForeignKey(p => p.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaMM>()
                .HasOne(p => p.LokacjaZ)
                .WithMany()
                .HasForeignKey(p => p.IdLokacjiZ)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaMM>()
                .HasOne(p => p.LokacjaDo)
                .WithMany()
                .HasForeignKey(p => p.IdLokacjiDo)
                .OnDelete(DeleteBehavior.Restrict);


            // --- Inwentaryzacja: FK restricty
            modelBuilder.Entity<Inwentaryzacja>()
                .HasOne(d => d.Magazyn)
                .WithMany()
                .HasForeignKey(d => d.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inwentaryzacja>()
                .HasOne(d => d.Utworzyl)
                .WithMany()
                .HasForeignKey(d => d.IdUtworzyl)
                .OnDelete(DeleteBehavior.Restrict);

            // PozycjaInwentaryzacji: produkt/lokacja restricty
            modelBuilder.Entity<PozycjaInwentaryzacji>()
                .HasOne(p => p.Produkt)
                .WithMany()
                .HasForeignKey(p => p.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaInwentaryzacji>()
                .HasOne(p => p.Lokacja)
                .WithMany()
                .HasForeignKey(p => p.IdLokacji)
                .OnDelete(DeleteBehavior.Restrict);


            // --- Rezerwacja: FK restricty
            modelBuilder.Entity<Rezerwacja>()
                .HasOne(r => r.Magazyn)
                .WithMany()
                .HasForeignKey(r => r.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rezerwacja>()
                .HasOne(r => r.Utworzyl)
                .WithMany()
                .HasForeignKey(r => r.IdUtworzyl)
                .OnDelete(DeleteBehavior.Restrict);

            // PozycjaRezerwacji: produkt/lokacja restricty
            modelBuilder.Entity<PozycjaRezerwacji>()
                .HasOne(p => p.Produkt)
                .WithMany()
                .HasForeignKey(p => p.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PozycjaRezerwacji>()
                .HasOne(p => p.Lokacja)
                .WithMany()
                .HasForeignKey(p => p.IdLokacji)
                .OnDelete(DeleteBehavior.Restrict);


            // --- Partie: FK restricty
            modelBuilder.Entity<Partia>()
                .HasOne(b => b.Produkt)
                .WithMany()
                .HasForeignKey(b => b.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Partia>()
                .HasOne(b => b.Dostawca)
                .WithMany()
                .HasForeignKey(b => b.IdDostawcy)
                .OnDelete(DeleteBehavior.Restrict);

            // --- AlertRules (RegulaAlertu): żadnych kaskad
            modelBuilder.Entity<RegulaAlertu>()
                .HasOne(r => r.Magazyn)
                .WithMany()
                .HasForeignKey(r => r.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegulaAlertu>()
                .HasOne(r => r.Produkt)
                .WithMany()
                .HasForeignKey(r => r.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Alerts (Alert): żadnych kaskad
            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Regula)
                .WithMany()
                .HasForeignKey(a => a.IdReguly)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Magazyn)
                .WithMany()
                .HasForeignKey(a => a.IdMagazynu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Produkt)
                .WithMany()
                .HasForeignKey(a => a.IdProduktu)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
