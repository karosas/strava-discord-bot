using System.Collections.Generic;

namespace StravaDiscordBot.Models
{
    public class CategoryResult
    {
        public string Name { get; set; }
        public List<SubCategoryResult> SubCategoryResults { get; set; }
    }

    public class SubCategoryResult
    {
        public string Name { get; set; }
        public List<ParticipantResult> OrderedParticipantResults { get; set; }
    }
}