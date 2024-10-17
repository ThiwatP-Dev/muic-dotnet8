namespace KeystoneLibrary.Models.DataModels.MasterTables
{
    public class AnnouncementTopic
    {
        public long AnnouncementId { get; set; }
        public long TopicId { get; set; }

        [JsonIgnore]
        public virtual Announcement Announcement { get; set; }

        [JsonIgnore]
        public virtual Topic Topic { get; set; }
    }
}