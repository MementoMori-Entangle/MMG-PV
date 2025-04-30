using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class PlayerCSV
    {
        [Name("UId")]
        public int UId { get; set; } = 0;
        [Name("Id")]
        public long Id { get; set; } = 0;
        [Name("No")]
        public int No { get; set; } = 0;
        [Name("World")]
        public int World { get; set; } = 0;
        [Name("GuildId")]
        public int GuildId { get; set; } = 0;
        [Name("GuildName")]
        public string GuildName { get; set; } = string.Empty;
        [Name("Name")]
        public string Name { get; set; } = string.Empty;
        [Name("ForceValue")]
        public long ForceValue { get; set; } = 0;
        [Name("VC")]
        public bool IsVC { get; set; } = false;
        [Name("VirtualityFormation")]
        public int VirtualityFormation { get; set; } = 0;
        [Name("VFCNum")]
        public int VFCNum { get; set; } = 0;
        [Name("GBGroupId")]
        public int GBGroupId { get; set; } = 0;
        [Name("Delete")]
        public bool IsDelete { get; set; } = false;
        public PlayerCSV() { }
    }
}
