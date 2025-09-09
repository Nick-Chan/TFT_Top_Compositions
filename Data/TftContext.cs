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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnitStat>().ToTable("UnitStats");
            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<TeamPlacement>().ToTable("TeamPlacements");
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
        public string Region { get; set; }
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
        public string Region { get; set; }
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
        public string Region { get; set; }
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
