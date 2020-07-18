namespace StravaDiscordBot.Models
{
    public class ParticipantResult
    {
        public ParticipantResult(LeaderboardParticipant participant, double value, string displayValue)
        {
            Participant = participant;
            Value = value;
            DisplayValue = displayValue;
        }

        public LeaderboardParticipant Participant { get; set; }
        public double Value { get; set; }
        public string DisplayValue { get; set; }
    }
}