using Microsoft.EntityFrameworkCore;

namespace TFT.Data
{
    public class TftContext : DbContext
    {
        public DbSet<TeamPlacement> TeamPlacements { get; set; }

        public TftContext(DbContextOptions<TftContext> options) : base(options)
        {

        }
    }

    public class TeamPlacement
    {
        public int Id { get; set; }
        public int Placement { get; set; }
        public string TeamComposition { get; set; }
    }
}
