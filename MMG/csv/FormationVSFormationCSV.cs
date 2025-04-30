using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class FormationVSFormationCSV
    {
        [Name("UId")]
        public long UId { get; set; }
        [Name("WinFormationId")]
        public int WinFormationId { get; set; }
        [Name("LoseFormationId")]
        public int LoseFormationId { get; set; }
        [Name("DebuffKONum")]
        public int DebuffKONum { get; set; }

        public FormationVSFormationCSV() { }
    }
}
