namespace MMG.Dto
{
    public class DiscordDto : Dto
    {
        public ulong Id { get; set; }
        public string UserName { get; set; }
        public long MMId { get; set; }
        public int RemoteControlPermission { get; set; }
        public ulong DedicatedChannelId { get; set; }
    }
}
