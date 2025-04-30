using CsvHelper.Configuration.Attributes;

namespace MMG.csv
{
    public class FormationCSV
    {
        [Name("Id")]
        public int Id { get; set; } = 0;
        [Name("PlayerId")]
        public long PlayerId { get; set; } = 0;
        [Name("SortNo")]
        public int SortNo { get; set; } = 0;
        [Name("Name")]
        public string Name { get; set; } = string.Empty;
        [Name("SubParty")]
        public bool IsSubParty { get; set; } = false;
        [Name("Debuff")]
        public bool IsDebuff { get; set; } = false;
        [Name("Description")]
        public string Description { get; set; } = string.Empty;
        [Name("GVS")]
        public bool IsGVS { get; set; } = true;
        public FormationCSV() { }
    }
}
