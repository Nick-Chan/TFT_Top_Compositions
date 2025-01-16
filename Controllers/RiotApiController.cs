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
        public async Task<IActionResult> GetTopPlayers(string region)
        {
            var data = await _riotApiService.GetTopPlayersAsync(region);
            return Ok(data);
        }

        [HttpGet("summoner/{summonerId}")]
        public async Task<IActionResult> GetSummonerDetails(string region, string summonerId)
        {
            var data = await _riotApiService.GetSummonerDetailsAsync(region, summonerId);
            return Ok(data);
        }

        [HttpGet("matches/{puuid}")]
        public async Task<IActionResult> GetMatchIds(string region, string puuid, int start = 0, long? startTime = null, int count = 999)
        {
            // Use provided startTime or default to 48 hours ago
            startTime ??= DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds();

            var data = await _riotApiService.GetMatchIdsAsync(region, puuid, start, startTime.Value, count);
            return Ok(data);
        }

        [HttpGet("match/{matchId}")]
        public async Task<IActionResult> GetMatchDetails(string region, string matchId)
        {
            var data = await _riotApiService.GetMatchDetailsAsync(region, matchId);
            return Ok(data);
        }

        [HttpGet("api-data/{region}")]
        public async Task<IActionResult> GetApiData(string region)
        {
            try
            {
                // Integers to be set
                int playerCount = 10; // Number of players to loop through
                int startTimeDays = -1; // Start time for history of matches

                // Initialize API rate limiter
                var rateLimiter = new ApiRateLimiter();
                int apiLogCount = 0;

                // Dictionary mapping for Traits
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

                // Dictionary mapping for Units
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
                    { "tft13_jinx", "Jinx" },
                    { "TFT13_KogMaw", "Kog'Maw" },
                    { "TFT13_LeBlanc", "LeBlanc" },
                    { "TFT13_Leona", "Leona" },
                    { "TFT13_Beardy", "Loris" },
                    { "TFT13_Lux", "Lux" },
                    { "TFT13_Shooter", "Maddie" },
                    { "TFT13_Malzahar", "Malzahar" },
                    { "TFT13_MissMage", "Mel" },
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
                    { "TFT13_Viktor", "Viktor" },
                    { "TFT13_Red", "Violet" },
                    { "TFT13_Vladimir", "Vladimir" },
                    { "TFT13_Warwick", "Warwick" },
                    { "TFT13_Zeri", "Zeri" },
                    { "TFT13_Ziggs", "Ziggs" },
                    { "TFT13_Zoe", "Zoe" },
                    { "TFT13_Zyra", "Zyra" }
                };

                // Dictionary mapping for Items
                var itemNameMap = new Dictionary<string, string>
                {
                    { "TFT_Item_AdaptiveHelm", "Adaptive Helm" },
                    { "TFT_Item_ArchangelsStaff", "Archangel's Staff" },
                    { "TFT_Item_Bloodthirster", "Bloodthirster" },
                    { "TFT_Item_BlueBuff", "Blue Buff" },
                    { "TFT_Item_BrambleVest", "Bramble Vest" },
                    { "TFT_Item_Crownguard", "Crownguard" },
                    { "TFT_Item_Deathblade", "Deathblade" },
                    { "TFT_Item_DragonsClaw", "Dragon's Claw" },
                    { "TFT_Item_GuardianAngel", "Edge of Night" },
                    { "TFT_Item_SpectralGauntlet", "Evenshroud" },
                    { "TFT_Item_GargoyleStoneplate", "Gargoyle Stoneplate" },
                    { "TFT_Item_MadredsBloodrazor", "Giant Slayer" },
                    { "TFT_Item_PowerGauntlet", "Guardbreaker" },
                    { "TFT_Item_GuinsoosRageblade", "Guinsoo's Rageblade" },
                    { "TFT_Item_UnstableConcoction", "Hand of Justice" },
                    { "TFT_Item_HextechGunblade", "Hextech Gunblade" },
                    { "TFT_Item_InfinityEdge", "Infinity Edge" },
                    { "TFT_Item_IonicSpark", "Ionic Spark" },
                    { "TFT_Item_JeweledGauntlet", "Jeweled Gauntlet" },
                    { "TFT_Item_LastWhisper", "Last Whisper" },
                    { "TFT_Item_Morellonomicon", "Morellonomicon" },
                    { "TFT_Item_Leviathan", "Nashor's Tooth" },
                    { "TFT_Item_FrozenHeart", "Protector's Vow" },
                    { "TFT_Item_Quicksilver", "Quicksilver" },
                    { "TFT_Item_RabadonsDeathcap", "Rabadon's Deathcap" },
                    { "TFT_Item_RapidFireCannon", "Red Buff" },
                    { "TFT_Item_Redemption", "Redemption" },
                    { "TFT_Item_RunaansHurricane", "Runaan's Hurricane" },
                    { "TFT_Item_SpearOfShojin", "Spear of Shojin" },
                    { "TFT_Item_StatikkShiv", "Statikk Shiv" },
                    { "TFT_Item_NightHarvester", "Steadfast Heart" },
                    { "TFT_Item_SteraksGage", "Sterak's Gage" },
                    { "TFT_Item_RedBuff", "Sunfire Cape" },
                    { "TFT_Item_ThiefsGloves", "Thief's Gloves" },
                    { "TFT_Item_TitansResolve", "Titan's Resolve" },
                    { "TFT_Item_WarmogsArmor", "Warmog's Armor" }
                };

                // Start time tracking for the entire process
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Clear the TeamPlacements table
                _context.TeamPlacements.RemoveRange(_context.TeamPlacements);
                _context.Items.RemoveRange(_context.Items);
                await _context.SaveChangesAsync();

                // Increment API call counter and get top players
                rateLimiter.ApiCallCount++;
                apiLogCount++;
                var jsonTopPlayersResponse = await _riotApiService.GetTopPlayersAsync(region);
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
                    var jsonSummonerDetailsResponse = await _riotApiService.GetSummonerDetailsAsync(region, summonerId);
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
                    var jsonMatchIdsResponse = await _riotApiService.GetMatchIdsAsync(region, puuid, 0, startTime, 999);

                    string[] matchIdsArray = jsonMatchIdsResponse;

                    foreach (var matchId in matchIdsArray)
                    {
                        await HandleRateLimitAsync(rateLimiter);
                        // Increment API call counter and get match details
                        rateLimiter.ApiCallCount++;
                        apiLogCount++;
                        var jsonMatchDetailsResponse = await _riotApiService.GetMatchDetailsAsync(region, matchId);
                        var matchDetails = JsonSerializer.Deserialize<Models.MatchIds>(jsonMatchDetailsResponse);

                        if (matchDetails?.info?.participants != null)
                        {
                            foreach (var participant in matchDetails.info.participants)
                            {
                                if (participant.puuid == puuid)
                                {
                                    string traitName = "";
                                    string unitCharacterIds = "";

                                    // Ensure we are only processing traits that are active
                                    var validTraits = participant.traits
                                        .Where(t => t.tier_current > 0)
                                        .OrderByDescending(t => t.tier_current)
                                        .ThenByDescending(t => t.num_units)
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

                                                // Add to UnitStats table

                                                var unitStats = new UnitStat
                                                {
                                                    Unit = mappedName,
                                                    Trait = traitName,
                                                    Items = unit.itemNames != null ? unit.itemNames.Count : 0,
                                                    Level = unit.tier,
                                                    Placement = participant.placement,
                                                    DateTime = DateTime.Now
                                                };

                                                _context.UnitStats.Add(unitStats);

                                                if (unit.itemNames != null)
                                                {
                                                    foreach (var item in unit.itemNames)
                                                    {
                                                        // Map item name & skip if the item is not in the map
                                                        if (!itemNameMap.TryGetValue(item, out var itemName))
                                                        {
                                                            continue;
                                                        }

                                                        if (item == "TFT_Item_ThiefsGloves")
                                                        {
                                                            var thiefGlovesItem = new Item
                                                            {
                                                                Unit = mappedName,
                                                                ItemName = itemName,
                                                                Placement = participant.placement,
                                                                DateTime = DateTime.Now
                                                            };

                                                            _context.Items.Add(thiefGlovesItem);

                                                            break;
                                                        }
                                                        else
                                                        {
                                                            var items = new Item
                                                            {
                                                                Unit = mappedName,
                                                                ItemName = itemName,
                                                                Placement = participant.placement,
                                                                DateTime = DateTime.Now
                                                            };

                                                            _context.Items.Add(items);
                                                        }  
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Fallback if the name is not found in the dictionary
                                            if (unit.character_id != "TFT13_JayceSummon" && unit.character_id != "TFT13_Sion")
                                            {
                                                unitCharacterIds += unit.character_id + ", ";

                                                if (unit.itemNames != null)
                                                {
                                                    foreach (var item in unit.itemNames)
                                                    {
                                                        // Map item name & skip if the item is not in the map
                                                        if (!itemNameMap.TryGetValue(item, out var itemName))
                                                        {
                                                            continue;
                                                        }

                                                        if (item == "TFT_Item_ThiefsGloves")
                                                        {
                                                            var thiefGlovesItem = new Item
                                                            {
                                                                Unit = unit.character_id,
                                                                ItemName = itemName,
                                                                Placement = participant.placement,
                                                                DateTime = DateTime.Now
                                                            };

                                                            _context.Items.Add(thiefGlovesItem);

                                                            break;
                                                        }
                                                        else
                                                        {
                                                            var items = new Item
                                                            {
                                                                Unit = unit.character_id,
                                                                ItemName = itemName,
                                                                Placement = participant.placement,
                                                                DateTime = DateTime.Now
                                                            };

                                                            _context.Items.Add(items);
                                                        }
                                                    }
                                                }
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
            const int shortLimit = 17;
            const int shortWindowSeconds = 1;
            const int longLimit = 90;
            const int longWindowSeconds = 120;
            const int bufferMilliseconds = 75; // Add a buffer

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
                        PlayRate = (double)g.Count() / totalGames,
                        Wins = g.Count(tp => tp.Placement == 1),
                        Top4 = g.Count(tp => tp.Placement <= 4)
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
                // Calculate the total number of games for the specific trait composition
                var totalTraitGames = await _context.TeamPlacements
                    .Where(tp => tp.TraitComposition == traitComposition)
                    .CountAsync();

                if (totalTraitGames == 0)
                {
                    return Ok(new { Message = "No games found for the specified trait composition." });
                }

                var results = await _context.TeamPlacements
                    .Where(tp => tp.TraitComposition == traitComposition)
                    .GroupBy(tp => tp.UnitComposition)
                    .Select(g => new
                    {
                        UnitComposition = g.Key,
                        Occurrences = g.Count(),
                        AvgPlacement = g.Average(tp => tp.Placement),
                        PlayRate = (double)g.Count() / totalTraitGames
                    })
                    .OrderByDescending(result => result.Occurrences)
                    .ThenBy(result => result.AvgPlacement)
                    .Take(8) // Limit to top x rows
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("best-items-by-placement")]
        public async Task<IActionResult> GetBestItemsPlacements()
        {
            try
            {
                var results = await _context.Items
                    .GroupBy(item => new { item.Unit, item.ItemName })
                    .Select(g => new
                    {
                        Unit = g.Key.Unit,
                        ItemName = g.Key.ItemName,
                        AvgPlacement = g.Average(i => i.Placement)
                    })
                    .GroupBy(result => result.Unit)
                    .Select(g => new
                    {
                        Unit = g.Key,
                        Items = g.OrderBy(r => r.AvgPlacement)
                                 .Take(10) // Limit to top 10 items per unit
                                 .Select(r => new
                                 {
                                     ItemName = r.ItemName,
                                     AvgPlacement = Math.Round(r.AvgPlacement, 2)
                                 })
                                 .ToList()
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        public class BestItemResult
        {
            public string Unit { get; set; }
            public List<string> ItemNames { get; set; }
        }

        [HttpGet("unit-stats")]
        public async Task<IActionResult> GetUnitStats(string trait)
        {
            try
            {
                var results = await _context.UnitStats
                    .Where(us => us.Trait == trait)
                    .GroupBy(us => us.Unit)
                    .Select(g => new
                    {
                        Unit = g.Key,
                        AvgItems = g.Average(us => us.Items),
                        AvgLevel = g.Average(us => us.Level),
                        AvgPlacement = g.Average(us => us.Placement)
                    })
                    .OrderByDescending(result => result.AvgItems)
                    .ThenBy(result => result.AvgPlacement)
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
