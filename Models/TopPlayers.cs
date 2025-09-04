using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TFT.Models
{
    public class TopPlayers
    {
        public string tier { get; set; }
        public string leagueId { get; set; }
        public string queue { get; set; }
        public string name { get; set; }
        public List<LeagueEntry> entries { get; set; }
    }

    public class LeagueEntry
    {
        public string puuid { get; set; }
        public int leaguePoints { get; set; }
        public string rank { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public bool veteran { get; set; }
        public bool inactive { get; set; }
        public bool freshBlood { get; set; }
        public bool hotStreak { get; set; }
    }
}
