namespace StravaDiscordBot.Models
{
        public class ParticipantResult
        {
            public ParticipantResult(LeaderboardParticipant participant, double value)
            {
                Participant = participant;
                Value = value;
            }

            public LeaderboardParticipant Participant { get; set; }
            public double Value { get; set; }
        }
}
