using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TFT.Services
{
    public class RiotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RiotApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["RiotApi:ApiKey"]; ; // API Key
        }

        // REGIONS: BR1, EUN1, EUW1, JP1, KR, LA1, LA2, ME1, NA1, OC1, PH2, RU, SG2, TH2, TR1, TW2, VN2

        // AMERICAS: BR1 (Brazil), LA1 (Latin America North), LA2 (Latin America South), NA1 (North America)

        // ASIA: JP1 (Japan), KR (Korea), TW2* (Taiwan, HK, Macao)

        // EUROPE: EUN1 (Europe Nordic & East), EUW1 (Europe West), RU (Russia), TR1* (Turkey), ME1* (Middle East)

        // SEA: OC1 (Oceania), PH2* (Philippines), SG2 (Singapore), TH2* (Thailand), VN2* (Vietnam)

        Dictionary<string, string> _region = new Dictionary<string, string>() {
            { "BR1", "AMERICAS"},
            { "LA1", "AMERICAS"},
            { "LA2", "AMERICAS"},
            { "NA1", "AMERICAS"},
            { "EUN1", "EUROPE"},
            { "EUW1", "EUROPE"},
            { "VN2", "SEA"}};

        public async Task<string> GetTopPlayersAsync(string region)
        {
            string url = $"https://{region}.api.riotgames.com/tft/league/v1/challenger?queue=RANKED_TFT&api_key={_apiKey}";
           
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetSummonerDetailsAsync(string region, string summonerId)
        {
            string url = $"https://{region}.api.riotgames.com/tft/summoner/v1/summoners/{summonerId}?api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string[]> GetMatchIdsAsync(string region, string puuid, int start, long startTime, int count)
        {
            string regionGroup = "";
            
            if (_region.ContainsKey(region))
            {
                regionGroup = _region[region];
            }
            else
            {}
            
            string url = $"https://{regionGroup}.api.riotgames.com/tft/match/v1/matches/by-puuid/{puuid}/ids?start={start}&startTime={startTime}&count={count}&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Deserialize the JSON response into a string array
            var matchIdsJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string[]>(matchIdsJson);
        }

        public async Task<string> GetMatchDetailsAsync(string region, string matchId)
        {
            string regionGroup = "";

            if (_region.ContainsKey(region))
            {
                regionGroup = _region[region];
            }
            else
            { }

            string url = $"https://{regionGroup}.api.riotgames.com/tft/match/v1/matches/{matchId}?api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
