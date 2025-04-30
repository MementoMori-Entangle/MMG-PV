using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class GuildCSV
    {
        [Name("Id")]
        public int Id { get; set; } = 0;
        [Name("No")]
        public int No { get; set; } = 0;
        [Name("World")]
        public int World { get; set; } = 0;
        [Name("Name")]
        public string Name { get; set; } = string.Empty;
        [Name("Remarks")]
        public string Remarks { get; set; } = string.Empty;
        [Name("Delete")]
        public bool IsDelete { get; set; } = false;
        public GuildCSV() { }
    }
}
