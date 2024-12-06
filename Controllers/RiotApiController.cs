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
            // Use provided startTime or default to 48 hours ago
            startTime ??= DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds();

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
                // Integers to be set
                int playerCount = 40; // Number of players to loop through
                int startTimeDays = -1; // Start time for history of matches

                // Initialize API rate limiter
                var rateLimiter = new ApiRateLimiter();
                int apiLogCount = 0;

                // Dictionary mapping for Traits & Units
                var traitNameMap = new Dictionary<string, string>
                {
                    { "TFT13_Academy", "Academy" },
                    { "TFT13_Ambusher", "Ambusher" },
                    { "TFT13_Martialist", "Artillerist" },
                    { "TFT13_Hextech", "Automata" },
                    { "TFT13_Cabal", "Black Rose" },
                    { "TFT13_Bruiser", "Bruiser" },
                    { "TFT13_Crime", "Chem-Baron" },
                    { "TFT13_Warband", "Conqueror" },
                    { "TFT13_Infused", "Dominator" },
                    { "TFT13_Ambassador", "Emissary" },
                    { "TFT13_Squad", "Enforcer" },
                    { "TFT13_Experiment", "Experiment" },
                    { "TFT13_Family", "Family" },
                    { "TFT13_Hoverboard", "Firelight" },
                    { "TFT13_FormSwapper", "Form Swapper" },
                    { "TFT13_HighRoller", "High Roller" },
                    { "TFT13_Pugilist", "Pit Fighter" },
                    { "TFT13_Challenger", "Quickstriker" },
                    { "TFT13_Rebel", "Rebel" },
                    { "TFT13_Scrap", "Scrap" },
                    { "TFT13_Titan", "Sentinel" },
                    { "TFT13_Sniper", "Sniper" },
                    { "TFT13_Sorcerer", "Sorcerer" },
                    { "TFT13_Teamup_UnlikelyDuo", "Unlikely Duo" },
                    { "TFT13_Invoker", "Visionary" },
                    { "TFT13_Watcher", "Watcher" },
                    { "TFT13_JunkerKing", "Junker King" }
                };

                var unitNameMap = new Dictionary<string, string>
                {
                    { "TFT13_Akali", "Akali" },
                    { "TFT13_Ambessa", "Ambessa" },
                    { "TFT13_Amumu", "Amumu" },
                    { "TFT13_Blitzcrank", "Blitzcrank" },
                    { "TFT13_Caitlyn", "Caitlyn" },
                    { "TFT13_Camille", "Camille" },
                    { "TFT13_Cassiopeia", "Cassiopeia" },
                    { "TFT13_Corki", "Corki" },
                    { "TFT13_Darius", "Darius" },
                    { "TFT13_DrMundo", "Dr Mundo" },
                    { "TFT13_Draven", "Draven" },
                    { "TFT13_Ekko", "Ekko" },
                    { "tft13_elise", "Elise" },
                    { "TFT13_Elise", "Elise" },
                    { "TFT13_Ezreal", "Ezreal" },
                    { "TFT13_Gangplank", "Gangplank" },
                    { "TFT13_Garen", "Garen" },
                    { "TFT13_Heimerdinger", "Heimerdinger" },
                    { "TFT13_Illaoi", "Illaoi" },
                    { "TFT13_Irelia", "Irelia" },
                    { "TFT13_Jayce", "Jayce" },
                    { "TFT13_Jinx", "Jinx" },
                    { "TFT13_KogMaw", "Kog'Maw" },
                    { "TFT13_LeBlanc", "LeBlanc" },
                    { "TFT13_Leona", "Leona" },
                    { "TFT13_Beardy", "Loris" },
                    { "TFT13_Lux", "Lux" },
                    { "TFT13_Shooter", "Maddie" },
                    { "TFT13_Malzahar", "Malzahar" },
                    { "TFT13_Mordekaiser", "Mordekaiser" },
                    { "TFT13_Morgana", "Morgana" },
                    { "TFT13_Nami", "Nami" },
                    { "TFT13_Nocturne", "Nocturne" },
                    { "TFT13_NunuWillump", "Nunu Willump" },
                    { "TFT13_Blue", "Powder" },
                    { "TFT13_Rell", "Rell" },
                    { "TFT13_RenataGlasc", "Renata Glasc" },
                    { "TFT13_Chainsaw", "Renni" },
                    { "TFT13_Rumble", "Rumble" },
                    { "TFT13_FlyGuy", "Scar" },
                    { "TFT13_Sett", "Sett" },
                    { "TFT13_Lieutenant", "Sevika" },
                    { "TFT13_Silco", "Silco" },
                    { "TFT13_Singed", "Singed" },
                    { "TFT13_Gremlin", "Smeech" },
                    { "TFT13_Fish", "Steb" },
                    { "tft13_swain", "Swain" },
                    { "TFT13_Swain", "Swain" },
                    { "TFT13_Tristana", "Tristana" },
                    { "TFT13_Trundle", "Trundle" },
                    { "TFT13_TwistedFate", "Twisted Fate" },
                    { "TFT13_Twitch", "Twitch" },
                    { "TFT13_Urgot", "Urgot" },
                    { "TFT13_Prime", "Vander" },
                    { "TFT13_Vex", "Vex" },
                    { "TFT13_Vi", "Vi" },
                    { "TFT13_Red", "Violet" },
                    { "TFT13_Vladimir", "Vladimir" },
                    { "TFT13_Zeri", "Zeri" },
                    { "TFT13_Ziggs", "Ziggs" },
                    { "TFT13_Zoe", "Zoe" },
                    { "TFT13_Zyra", "Zyra" }
                };

                // Start time tracking for the entire process
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Clear the TeamPlacements table
                _context.TeamPlacements.RemoveRange(_context.TeamPlacements);
                await _context.SaveChangesAsync();

                // Increment API call counter and get top players
                rateLimiter.ApiCallCount++;
                apiLogCount++;
                var jsonTopPlayersResponse = await _riotApiService.GetTopPlayersAsync();
                var players = JsonSerializer.Deserialize<Models.TopPlayers>(jsonTopPlayersResponse);

                var topSummonerIds = players?.entries.Take(playerCount).Select(entry => entry.summonerId).ToList();

                // If no players are found
                if (topSummonerIds == null || !topSummonerIds.Any())
                {
                    return NotFound("No players found.");
                }

                // Iterate through the summonerIds and process each one
                foreach (var summonerId in topSummonerIds)
                {
                    await HandleRateLimitAsync(rateLimiter);
                    // Increment API call counter and get summoner details
                    rateLimiter.ApiCallCount++;
                    apiLogCount++;
                    var jsonSummonerDetailsResponse = await _riotApiService.GetSummonerDetailsAsync(summonerId);
                    var summonerDetails = JsonSerializer.Deserialize<Models.SummonerDetails>(jsonSummonerDetailsResponse);

                    // Get the puuid of the player
                    var puuid = summonerDetails?.puuid;
                    if (string.IsNullOrEmpty(puuid))
                    {
                        continue;
                    }

                    await HandleRateLimitAsync(rateLimiter);
                    // Increment API call counter and get match IDs
                    rateLimiter.ApiCallCount++;
                    apiLogCount++;
                    long startTime = DateTimeOffset.UtcNow.AddDays(startTimeDays).ToUnixTimeSeconds();
                    var jsonMatchIdsResponse = await _riotApiService.GetMatchIdsAsync(puuid, 0, startTime, 999);

                    string[] matchIdsArray = jsonMatchIdsResponse;

                    foreach (var matchId in matchIdsArray)
                    {
                        await HandleRateLimitAsync(rateLimiter);
                        // Increment API call counter and get match details
                        rateLimiter.ApiCallCount++;
                        apiLogCount++;
                        var jsonMatchDetailsResponse = await _riotApiService.GetMatchDetailsAsync(matchId);
                        var matchDetails = JsonSerializer.Deserialize<Models.MatchIds>(jsonMatchDetailsResponse);

                        if (matchDetails?.info?.participants != null)
                        {
                            foreach (var participant in matchDetails.info.participants)
                            {
                                if (participant.puuid == puuid)
                                {
                                    string traitName = "";
                                    string unitCharacterIds = "";

                                    // Ensure we are only processing traits that meet the criteria (num_units > 2)
                                    var validTraits = participant.traits
                                        .Where(t => t.num_units > 3)
                                        .ToList();

                                    // Handle the first trait
                                    if (validTraits.Count > 0)
                                    {
                                        var firstTrait = validTraits[0]; // First trait remains in position
                                        if (traitNameMap.TryGetValue(firstTrait.name, out var mappedFirstName))
                                        {
                                            traitName += mappedFirstName + " ";
                                        }
                                        else
                                        {
                                            traitName += firstTrait.name + " ";
                                        }

                                        // Process the remaining traits (sorted alphabetically by name)
                                        var remainingTraits = validTraits.Skip(1)
                                            .OrderBy(t => traitNameMap.TryGetValue(t.name, out var mappedName) ? mappedName : t.name)
                                            .ToList();

                                        foreach (var trait in remainingTraits)
                                        {
                                            if (traitNameMap.TryGetValue(trait.name, out var mappedName))
                                            {
                                                traitName += mappedName + " ";
                                            }
                                            else
                                            {
                                                traitName += trait.name + " ";
                                            }
                                        }
                                    }

                                    // Trim trailing spaces
                                    traitName = traitName.TrimEnd();

                                    // Append unit character IDs
                                    foreach (var unit in participant.units)
                                    {
                                        if (unitNameMap.TryGetValue(unit.character_id, out var mappedName))
                                        {
                                            if (unit.character_id != "TFT13_JayceSummon" && unit.character_id != "TFT13_Sion")
                                            {
                                                unitCharacterIds += mappedName + ", ";
                                            }
                                        }
                                        else
                                        {
                                            // Fallback if the name is not found in the dictionary
                                            if (unit.character_id != "TFT13_JayceSummon" && unit.character_id != "TFT13_Sion")
                                            {
                                                unitCharacterIds += unit.character_id + ", ";
                                            }
                                        }
                                    }
                                    unitCharacterIds = unitCharacterIds.TrimEnd(',', ' ');

                                    // Only add to the database if traitName is not null or empty
                                    if (!string.IsNullOrEmpty(traitName))
                                    {
                                        // Create a new TeamPlacement object
                                        var teamPlacement = new TeamPlacement
                                        {
                                            Placement = participant.placement,
                                            TraitComposition = traitName,
                                            UnitComposition = unitCharacterIds
                                        };

                                        // Add the object to the database
                                        _context.TeamPlacements.Add(teamPlacement);
                                    }
                                }
                            }
                        }
                    }
                }

                // Save all changes to the database
                await _context.SaveChangesAsync();

                // Stop time tracking
                stopwatch.Stop();

                // Return API call count and elapsed time
                var elapsedTime = stopwatch.Elapsed;
                return Ok(new
                {
                    Message = "Placement saved successfully.",
                    ApiCalls = apiLogCount,
                    ElapsedTime = elapsedTime.ToString(@"hh\:mm\:ss\.fff")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        public class ApiRateLimiter
        {
            public int ApiCallCount { get; set; } = 0;
            public DateTime RateLimitStartTime { get; set; } = DateTime.UtcNow;
        }

        private async Task HandleRateLimitAsync(ApiRateLimiter rateLimiter)
        {
            /* 
                --Rate limits--
                (Development API Key)
                20 requests every 1 seconds(s)
                100 requests every 2 minutes(s)
            */
            const int shortLimit = 18;
            const int shortWindowSeconds = 1;
            const int longLimit = 90;
            const int longWindowSeconds = 120;
            const int bufferMilliseconds = 100; // Add a buffer of 100ms for safety

            // Check short window (20 requests per second)
            if (rateLimiter.ApiCallCount % shortLimit == 0)
            {
                TimeSpan elapsed = DateTime.UtcNow - rateLimiter.RateLimitStartTime;
                if (elapsed.TotalSeconds < shortWindowSeconds)
                {
                    var delay = TimeSpan.FromSeconds(shortWindowSeconds) - elapsed + TimeSpan.FromMilliseconds(bufferMilliseconds);
                    await Task.Delay(delay);
                }
            }

            // Check long window (100 requests per 2 minutes)
            if (rateLimiter.ApiCallCount % longLimit == 0)
            {
                TimeSpan elapsed = DateTime.UtcNow - rateLimiter.RateLimitStartTime;
                if (elapsed.TotalSeconds < longWindowSeconds)
                {
                    var delay = TimeSpan.FromSeconds(longWindowSeconds) - elapsed + TimeSpan.FromMilliseconds(bufferMilliseconds);
                    await Task.Delay(delay);
                }
            }

            // Reset rate limit start time if it has passed the long window
            if ((DateTime.UtcNow - rateLimiter.RateLimitStartTime).TotalSeconds >= longWindowSeconds)
            {
                rateLimiter.RateLimitStartTime = DateTime.UtcNow;
                rateLimiter.ApiCallCount = 0; // Reset counter after a full window
            }
        }

        [HttpGet("average-placements-by-traits")]
        public async Task<IActionResult> GetAverageTraitPlacements()
        {
            try
            {
                // Calculate the total number of games
                var totalGames = await _context.TeamPlacements.CountAsync();

                if (totalGames == 0)
                {
                    return Ok(new { Message = "No games found in the database." });
                }

                // Execute the query to calculate average placement and play rate
                var results = await _context.TeamPlacements
                    .GroupBy(tp => tp.TraitComposition)
                    .Where(g => g.Count() > 3) // Only include traits with more than 3 occurrences
                    .Select(g => new
                    {
                        TeamComposition = g.Key,
                        AvgPlacement = g.Average(tp => tp.Placement),
                        Occurrences = g.Count(),
                        PlayRate = (double)g.Count() / totalGames // Calculate play rate
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


        [HttpGet("unit-compositions-by-traits")]
        public async Task<IActionResult> GetUnitCompositionsByTraits(string traitComposition)
        {
            try
            {
                // Calculate total number of games
                var totalGames = await _context.TeamPlacements.CountAsync();

                if (totalGames == 0)
                {
                    return Ok(new { Message = "No games found in the database." });
                }

                var results = await _context.TeamPlacements
                    .Where(tp => tp.TraitComposition == traitComposition)
                    .GroupBy(tp => tp.UnitComposition)
                    .Select(g => new
                    {
                        UnitComposition = g.Key,
                        Occurrences = g.Count(),
                        AvgPlacement = g.Average(tp => tp.Placement),
                        PlayRate = (double)g.Count() / totalGames // Calculate play rate
                    })
                    .OrderByDescending(result => result.Occurrences)
                    .ThenBy(result => result.AvgPlacement)
                    .Take(5) // Limit to top 5 rows
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
