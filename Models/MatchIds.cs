using System;
using System.Collections.Generic;

namespace TFT.Models
{
    public class MatchIds
    {
        public Metadata metadata { get; set; }
        public Info info { get; set; }
    }

    public class Metadata
    {
        public string dataVersion { get; set; }
        public string matchId { get; set; }
        public List<string> participants { get; set; }
    }

    public class Info
    {
        public string endOfGameResult { get; set; }
        public long gameCreation { get; set; }
        public long gameId { get; set; }
        public long gameDatetime { get; set; }
        public double gameLength { get; set; }
        public string gameVersion { get; set; }
        public int mapId { get; set; }
        public List<Participant> participants { get; set; }
        public int queueId { get; set; }
        public string tftGameType { get; set; }
        public string tftSetCoreName { get; set; }
        public int tftSetNumber { get; set; }
    }

    public class Participant
    {
        public Companion companion { get; set; }
        public int goldLeft { get; set; }
        public int lastRound { get; set; }
        public int level { get; set; }
        public Dictionary<string, int> missions { get; set; }
        public int placement { get; set; }
        public int playersEliminated { get; set; }
        public string puuid { get; set; }
        public string riotIdGameName { get; set; }
        public string riotIdTagline { get; set; }
        public double timeEliminated { get; set; }
        public int totalDamageToPlayers { get; set; }
        public List<Trait> traits { get; set; }
        public List<Unit> units { get; set; }
        public bool win { get; set; }
    }

    public class Companion
    {
        public string contentId { get; set; }
        public int itemId { get; set; }
        public int SkinId { get; set; }
        public string species { get; set; }
    }

    public class Trait
    {
        public string name { get; set; }
        public int numUnits { get; set; }
        public int style { get; set; }
        public int tierCurrent { get; set; }
        public int tierTotal { get; set; }
    }

    public class Unit
    {
        public string character_id { get; set; }
        public List<string> itemNames { get; set; }
        public string name { get; set; }
        public int rarity { get; set; }
        public int tier { get; set; }
    }
}
