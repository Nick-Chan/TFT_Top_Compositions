using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TFT.Data
{
    public class TftContext : DbContext
    {
        public DbSet<TeamPlacement> TeamPlacements { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<UnitStat> UnitStats { get; set; }

        public TftContext(DbContextOptions<TftContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tft_analyzer;Username=tft_user;Password=tftpassword123");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("tft_europe"); // Default schema

            modelBuilder.Entity<TeamPlacement>().ToTable("TeamPlacements", "tft_europe");
            modelBuilder.Entity<Item>().ToTable("Items", "tft_europe");
            modelBuilder.Entity<UnitStat>().ToTable("UnitStats", "tft_europe");

            modelBuilder.Entity<TeamPlacement>().ToTable("TeamPlacements", "tft_americas");
            modelBuilder.Entity<Item>().ToTable("Items", "tft_americas");
            modelBuilder.Entity<UnitStat>().ToTable("UnitStats", "tft_americas");

            modelBuilder.Entity<TeamPlacement>().ToTable("TeamPlacements", "tft_sea");
            modelBuilder.Entity<Item>().ToTable("Items", "tft_sea");
            modelBuilder.Entity<UnitStat>().ToTable("UnitStats", "tft_sea");

            modelBuilder.Entity<TeamPlacement>().ToTable("TeamPlacements", "tft_asia");
            modelBuilder.Entity<Item>().ToTable("Items", "tft_asia");
            modelBuilder.Entity<UnitStat>().ToTable("UnitStats", "tft_asia");

            modelBuilder.Entity<GlobalTeamPlacement>().ToView("GlobalTeamPlacements").HasNoKey();
        }
    }

    public class TeamPlacement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Placement { get; set; }
        public string TraitComposition { get; set; }
        public string UnitComposition { get; set; }
    }

    public class Item
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Unit { get; set; }
        public string ItemName { get; set; }
        public int Placement { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class UnitStat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Unit { get; set; }
        public string Trait { get; set; }
        public int Items { get; set; }
        public int Placement { get; set; }
        public int Level { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class GlobalTeamPlacement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Region { get; set; }
        public int Placement { get; set; }
        public string TraitComposition { get; set; }
        public string UnitComposition { get; set; }
    }

}
