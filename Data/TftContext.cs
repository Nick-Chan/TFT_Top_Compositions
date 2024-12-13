using Microsoft.EntityFrameworkCore;

namespace TFT.Data
{
    public class TftContext : DbContext
    {
        public DbSet<TeamPlacement> TeamPlacements { get; set; }
        public DbSet<Item> Items { get; set; }

        public TftContext(DbContextOptions<TftContext> options) : base(options)
        {

        }
    }

    public class TeamPlacement
    {
        public int Id { get; set; }
        public int Placement { get; set; }
        public string TraitComposition { get; set; }
        public string UnitComposition { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Unit { get; set; }
        public string ItemName { get; set; }
        public int Placement { get; set; }
        public DateTime DateTime { get; set; }
    }
}
