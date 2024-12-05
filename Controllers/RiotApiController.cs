using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using TFT.Services;

using TFT.Models;
using TFT.Data;
using Polly;
using Microsoft.EntityFrameworkCore;


namespace TFT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiotApiController : Controller
    {
        private readonly RiotApiService _riotApiService;
        private readonly TftContext _context;

        public RiotApiController(RiotApiService riotApiService, TftContext context)
        {
            _riotApiService = riotApiService;
            _context = context;
        }

        [HttpGet("top-players")]
        public async Task<IActionResult> GetTopPlayers()
        {
            var data = await _riotApiService.GetTopPlayersAsync();
            return Ok(data);
        }

        [HttpGet("summoner/{summonerId}")]
        public async Task<IActionResult> GetSummonerDetails(string summonerId)
        {
            var data = await _riotApiService.GetSummonerDetailsAsync(summonerId);
            return Ok(data);
        }

        [HttpGet("matches/{puuid}")]
        public async Task<IActionResult> GetMatchIds(string puuid, int start = 0, long? startTime = null, int count = 999)
        {
            // Use provided startTime or default to 24 hours ago
            startTime ??= DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();

            var data = await _riotApiService.GetMatchIdsAsync(puuid, start, startTime.Value, count);
            return Ok(data);
        }

        [HttpGet("match/{matchId}")]
        public async Task<IActionResult> GetMatchDetails(string matchId)
        {
            var data = await _riotApiService.GetMatchDetailsAsync(matchId);
            return Ok(data);
        }

        [HttpGet("api-data")]
        public async Task<IActionResult> GetApiData()
        {
            try
            {
                // GET call TopPlayers API and deserialize the response into a list of players
                var jsonTopPlayersResponse = await _riotApiService.GetTopPlayersAsync();
                var players = JsonSerializer.Deserialize<Models.TopPlayers>(jsonTopPlayersResponse);

                // Get the summonerId of the first player
                var firstSummonerId = players?.entries.FirstOrDefault()?.summonerId;

                if (string.IsNullOrEmpty(firstSummonerId))
                {
                    return NotFound("No players found.");
                }

                // Get the puuid from the summonerId, GET call SummonerDetails API and deserialize the response
                var jsonSummonerDetailsResponse = await _riotApiService.GetSummonerDetailsAsync(firstSummonerId);
                var summonerDetails = JsonSerializer.Deserialize<Models.SummonerDetails>(jsonSummonerDetailsResponse);

                // Get the puuid of the player
                var puuid = summonerDetails?.puuid;

                if (string.IsNullOrEmpty(puuid))
                {
                    return NotFound("No puuid found.");
                }

                // GET call MatchIds API and deserialize the response
                long startTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
                var jsonMatchIdsResponse = await _riotApiService.GetMatchIdsAsync(puuid, 0, startTime, 999);

                string[] matchIdsArray = jsonMatchIdsResponse;

                foreach (var matchId in matchIdsArray)
                {
                    // GET call MatchDetails API and deserialize the response
                    var jsonMatchDetailsResponse = await _riotApiService.GetMatchDetailsAsync(matchId);
                    var matchDetails = JsonSerializer.Deserialize<Models.MatchIds>(jsonMatchDetailsResponse);

                    if (matchDetails?.info?.participants != null)
                    {
                        foreach (var participant in matchDetails.info.participants)
                        {
                            if (participant.puuid == puuid)
                            {
                                string characterIds = "";

                                foreach (var unit in participant.units)
                                {
                                    characterIds += unit.character_id + ", ";
                                }

                                // Remove trailing comma and space
                                characterIds = characterIds.TrimEnd(',', ' ');

                                // Create a new TeamPlacement object
                                var teamPlacement = new TeamPlacement
                                {
                                    Placement = participant.placement,
                                    TeamComposition = characterIds
                                };

                                // Add the object to the database
                                _context.TeamPlacements.Add(teamPlacement);
                            }
                        }

                        return NotFound("Matching puuid not found");
                    }
                    else
                    {
                        return NotFound("Participants array in null/empty");
                    }
                }

                await _context.SaveChangesAsync();

                return Ok("Placement saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("average-placements")]
        public async Task<IActionResult> GetAveragePlacements()
        {
            try
            {
                // Execute the query to calculate average placement
                var results = await _context.TeamPlacements
                    .GroupBy(tp => tp.TeamComposition)
                    .Select(g => new
                    {
                        TeamComposition = g.Key,
                        AvgPlacement = g.Average(tp => tp.Placement),
                        Occurrences = g.Count()
                    })
                    .OrderBy(result => result.AvgPlacement)
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }
}
