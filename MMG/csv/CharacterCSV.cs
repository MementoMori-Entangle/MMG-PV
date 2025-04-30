using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class CharacterCSV
    {
        [Name("Id")]
        public int Id { get; set; } = 0;
        [Name("FormationId")]
        public int FormationId { get; set; } = 0;
        [Name("PlayerId")]
        public long PlayerId { get; set; } = 0;
        [Name("SortNo")]
        public int SortNo { get; set; } = 0;
        [Name("Name")]
        public string Name { get; set; } = string.Empty;
        [Name("Rarity")]
        public int Rarity { get; set; } = 0;
        [Name("Level")]
        public int Level { get; set; } = 0;
        [Name("ForceValue")]
        public long ForceValue { get; set; } = 0;
        [Name("SpeedRune1")]
        public int SpeedRune1 { get; set; } = 0;
        [Name("SpeedRune2")]
        public int SpeedRune2 { get; set; } = 0;
        [Name("SpeedRune3")]
        public int SpeedRune3 { get; set; } = 0;
        [Name("PiercingRune1")]
        public int PiercingRune1 { get; set; } = 0;
        [Name("PiercingRune2")]
        public int PiercingRune2 { get; set; } = 0;
        [Name("PiercingRune3")]
        public int PiercingRune3 { get; set; } = 0;
        [Name("LRNum")]
        public int LRNum { get; set; } = 0;
        [Name("URNum")]
        public int URNum { get; set; } = 0;
        [Name("SSRNum")]
        public int SSRNum { get; set; } = 0;
        [Name("SRNum")]
        public int SRNum { get; set; } = 0;
        [Name("RNum")]
        public int RNum { get; set; } = 0;
        [Name("EWeapon")]
        public int EWeapon { get; set; } = 0;

        public CharacterCSV() { }
    }
}
