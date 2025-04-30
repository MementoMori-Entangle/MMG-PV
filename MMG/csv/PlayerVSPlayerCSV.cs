using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class PlayerVSPlayerCSV
    {
        [Name("UId")]
        public long UId { get; set; }
        [Name("WinPlayerId")]
        public long WinPlayerId { get; set; }
        [Name("LosePlayerId")]
        public long LosePlayerId { get; set; }

        public PlayerVSPlayerCSV() { }
    }
}
