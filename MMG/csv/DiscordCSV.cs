using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class DiscordCSV
    {
        [Name("Id")]
        public ulong Id { get; set; } = 0;
        [Name("UserName")]
        public string UserName { get; set; } = string.Empty;
        [Name("MMId")]
        public long MMId { get; set; } = 0;
        [Name("RemoteControlPermission")]
        public int RemoteControlPermission { get; set; } = 0;
        [Name("DedicatedChannelId")]
        public ulong DedicatedChannelId { get; set; } = 0;

        public DiscordCSV() { }
    }
}
