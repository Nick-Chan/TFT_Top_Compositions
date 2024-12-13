using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TFT.Services
{
    public class RiotApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public RiotApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = "RGAPI-11dbe8c1-ad2e-42f4-b18e-54c8ba34f670"; // API Key
        }

        public async Task<string> GetTopPlayersAsync()
        {
            string url = $"https://euw1.api.riotgames.com/tft/league/v1/challenger?queue=RANKED_TFT&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetSummonerDetailsAsync(string summonerId)
        {
            string url = $"https://euw1.api.riotgames.com/tft/summoner/v1/summoners/{summonerId}?api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string[]> GetMatchIdsAsync(string puuid, int start, long startTime, int count)
        {
            string url = $"https://europe.api.riotgames.com/tft/match/v1/matches/by-puuid/{puuid}/ids?start={start}&startTime={startTime}&count={count}&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Deserialize the JSON response into a string array
            var matchIdsJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<string[]>(matchIdsJson);
        }

        public async Task<string> GetMatchDetailsAsync(string matchId)
        {
            string url = $"https://europe.api.riotgames.com/tft/match/v1/matches/{matchId}?api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
