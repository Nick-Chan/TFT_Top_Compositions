using System.Text.Json;

namespace TFT.Services
{
    public class PollingService
    {
        private readonly RiotApiService _riotApiService;

        public PollingService(RiotApiService riotApiService)
        {
            _riotApiService = riotApiService;
        }

        public async Task<string> GetFirstPlayerSummonerIdAsync()
        {
            var jsonResponse = await _riotApiService.GetTopPlayersAsync();

            // Deserialize the response into a list of players
            var players = JsonSerializer.Deserialize<List<TopPlayer>>(jsonResponse);

            // Return the summonerId of the first player, or null if the list is empty
            return players?.FirstOrDefault()?.SummonerId;
        }
    }

    // Example model for deserialization
    public class TopPlayer
    {
        public string SummonerId { get; set; }
        public string RatedTier { get; set; }
        public int RatedRating { get; set; }
        public int Wins { get; set; }
        public int PreviousUpdateLadderPosition { get; set; }
    }
}
