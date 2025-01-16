using System.Text.Json;

namespace TFT.Services
{
    public class PollingService
    {
        //TBA
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
