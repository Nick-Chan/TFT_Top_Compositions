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
        public string data_version { get; set; }
        public string match_id { get; set; }
        public List<string> participants { get; set; }
    }

    public class Info
    {
        public string endOfGameResult { get; set; }
        public long gameCreation { get; set; }
        public long gameId { get; set; }
        public long game_datetime { get; set; }
        public double game_length { get; set; }
        public string game_version { get; set; }
        public int mapId { get; set; }
        public List<Participant> participants { get; set; }
        public int queue_id { get; set; }
        public string tft_game_type { get; set; }
        public string tft_set_core_name { get; set; }
        public int tft_set_number { get; set; }
    }

    public class Participant
    {
        public Companion companion { get; set; }
        public int gold_left { get; set; }
        public int last_round { get; set; }
        public int level { get; set; }
        public Dictionary<string, int> missions { get; set; }
        public int placement { get; set; }
        public int players_eliminated { get; set; }
        public string puuid { get; set; }
        public string riotIdGameName { get; set; }
        public string riotIdTagline { get; set; }
        public double time_eliminated { get; set; }
        public int total_damage_to_players { get; set; }
        public List<Trait> traits { get; set; }
        public List<Unit> units { get; set; }
        public bool win { get; set; }
    }

    public class Companion
    {
        public string content_ID { get; set; }
        public int item_ID { get; set; }
        public int skin_ID { get; set; }
        public string species { get; set; }
    }

    public class Trait
    {
        public string name { get; set; }
        public int num_units { get; set; }
        public int style { get; set; }
        public int tier_current { get; set; }
        public int tier_total { get; set; }
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
