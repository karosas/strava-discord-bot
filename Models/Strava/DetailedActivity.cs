using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StravaDiscordBot.Models.Strava
{
    public class DetailedActivity
    {
        [JsonProperty("resource_state")]
        public long? ResourceState { get; set; }

        [JsonProperty("athlete")]
        public AthleteShort Athlete { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("distance")]
        public double? Distance { get; set; }

        [JsonProperty("moving_time")]
        public long? MovingTime { get; set; }

        [JsonProperty("elapsed_time")]
        public long? ElapsedTime { get; set; }

        [JsonProperty("total_elevation_gain")]
        public long? TotalElevationGain { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("workout_type")]
        public object WorkoutType { get; set; }

        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("upload_id")]
        public double? UploadId { get; set; }

        [JsonProperty("start_date")]
        public DateTimeOffset? StartDate { get; set; }

        [JsonProperty("start_date_local")]
        public DateTimeOffset? StartDateLocal { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("utc_offset")]
        public long? UtcOffset { get; set; }

        [JsonProperty("start_latlng")]
        public object StartLatlng { get; set; }

        [JsonProperty("end_latlng")]
        public object EndLatlng { get; set; }

        [JsonProperty("location_city")]
        public object LocationCity { get; set; }

        [JsonProperty("location_state")]
        public object LocationState { get; set; }

        [JsonProperty("location_country")]
        public string LocationCountry { get; set; }

        [JsonProperty("start_latitude")]
        public object StartLatitude { get; set; }

        [JsonProperty("start_longitude")]
        public object StartLongitude { get; set; }

        [JsonProperty("achievement_count")]
        public long? AchievementCount { get; set; }

        [JsonProperty("kudos_count")]
        public long? KudosCount { get; set; }

        [JsonProperty("comment_count")]
        public long? CommentCount { get; set; }

        [JsonProperty("athlete_count")]
        public long? AthleteCount { get; set; }

        [JsonProperty("photo_count")]
        public long? PhotoCount { get; set; }

        [JsonProperty("trainer")]
        public bool? Trainer { get; set; }

        [JsonProperty("commute")]
        public bool? Commute { get; set; }

        [JsonProperty("manual")]
        public bool? Manual { get; set; }

        [JsonProperty("private")]
        public bool? Private { get; set; }

        [JsonProperty("flagged")]
        public bool? Flagged { get; set; }

        [JsonProperty("gear_id")]
        public string GearId { get; set; }

        [JsonProperty("from_accepted_tag")]
        public bool? FromAcceptedTag { get; set; }

        [JsonProperty("average_speed")]
        public double? AverageSpeed { get; set; }

        [JsonProperty("max_speed")]
        public long? MaxSpeed { get; set; }

        [JsonProperty("average_cadence")]
        public double? AverageCadence { get; set; }

        [JsonProperty("average_watts")]
        public double? AverageWatts { get; set; }

        [JsonProperty("weighted_average_watts")]
        public long? WeightedAverageWatts { get; set; }

        [JsonProperty("kilojoules")]
        public double? Kilojoules { get; set; }

        [JsonProperty("device_watts")]
        public bool? DeviceWatts { get; set; }

        [JsonProperty("has_heartrate")]
        public bool? HasHeartrate { get; set; }

        [JsonProperty("average_heartrate")]
        public double? AverageHeartrate { get; set; }

        [JsonProperty("max_heartrate")]
        public long? MaxHeartrate { get; set; }

        [JsonProperty("max_watts")]
        public long? MaxWatts { get; set; }

        [JsonProperty("pr_count")]
        public long? PrCount { get; set; }

        [JsonProperty("total_photo_count")]
        public long? TotalPhotoCount { get; set; }

        [JsonProperty("has_kudoed")]
        public bool? HasKudoed { get; set; }

        [JsonProperty("suffer_score")]
        public long? SufferScore { get; set; }
    }
}
