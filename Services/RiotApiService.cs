using System.Net.Http;
using System.Text.Json;

namespace TFT.Services
{
    public class RiotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // REGIONS: BR1, EUN1, EUW1, JP1, KR, LA1, LA2, ME1, NA1, OC1, PH2, RU, SG2, TH2, TR1, TW2, VN2

        // AMERICAS: BR1 (Brazil), LA1 (Latin America North), LA2 (Latin America South), NA1 (North America)

        // ASIA: JP1 (Japan), KR (Korea), TW2* (Taiwan, HK, Macao)

        // EUROPE: EUN1 (Europe Nordic & East), EUW1 (Europe West), RU (Russia), TR1* (Turkey), ME1* (Middle East)

        // SEA: OC1 (Oceania), PH2* (Philippines), SG2 (Singapore), TH2* (Thailand), VN2* (Vietnam)

        // Map platform to region group (for match/account APIs)
        private static readonly Dictionary<string, string> RegionGroups = new()
        {
            { "BR1", "americas" },
            { "LA1", "americas" },
            { "LA2", "americas" },
            { "NA1", "americas" },
            { "EUN1", "europe" },
            { "EUW1", "europe" },
            { "RU", "europe" },
            { "TR1", "europe" },
            { "KR", "asia" },
            { "JP1", "asia" },
            { "TW2", "asia" },
            { "VN2", "sea" },
            { "SG2", "sea" },
            { "PH2", "sea" },
            { "TH2", "sea" },
            { "OC1", "sea" },
            { "ME1", "europe" } // special case
        };

        public RiotApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["RiotApi:ApiKey"] ?? throw new InvalidOperationException("Riot API key missing");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _apiKey);
        }

        // Platform-routed: Challenger league
        public async Task<string> GetTopPlayersAsync(string platform)
        {
            platform = platform.ToLowerInvariant();
            var url = $"https://{platform}.api.riotgames.com/tft/league/v1/challenger?queue=RANKED_TFT";

            using var resp = await _httpClient.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Riot API error {resp.StatusCode}: {body}");

            return body;
        }

        // Platform-routed: Summoner details
        public async Task<string> GetSummonerDetailsAsync(string platform, string summonerId)
        {
            platform = platform.ToLowerInvariant();
            var url = $"https://{platform}.api.riotgames.com/tft/summoner/v1/summoners/by-puuid/{summonerId}";

            using var resp = await _httpClient.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Riot API error {resp.StatusCode}: {body}");

            return body;
        }

        // Regional-routed: Match IDs
        public async Task<string[]> GetMatchIdsAsync(string platform, string puuid, int start, long startTime, int count)
        {
            platform = platform.ToUpperInvariant(); // our map keys are uppercase
            if (!RegionGroups.TryGetValue(platform, out var regionGroup))
                throw new ArgumentException($"Unknown platform: {platform}");

            var url = $"https://{regionGroup}.api.riotgames.com/tft/match/v1/matches/by-puuid/{puuid}/ids?start={start}&startTime={startTime}&count={count}";

            using var resp = await _httpClient.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Riot API error {resp.StatusCode}: {body}");

            return JsonSerializer.Deserialize<string[]>(body) ?? Array.Empty<string>();
        }

        // Regional-routed: Match details
        public async Task<string> GetMatchDetailsAsync(string platform, string matchId)
        {
            platform = platform.ToUpperInvariant();
            if (!RegionGroups.TryGetValue(platform, out var regionGroup))
                throw new ArgumentException($"Unknown platform: {platform}");

            var url = $"https://{regionGroup}.api.riotgames.com/tft/match/v1/matches/{matchId}";

            using var resp = await _httpClient.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Riot API error {resp.StatusCode}: {body}");

            return body;
        }

    }
}
