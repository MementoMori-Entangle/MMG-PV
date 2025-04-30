namespace MMG.Dto
{
    public class FormationVSFormationDto : Dto
    {
        public long UId { get; set; }
        public int WinFormationId { get; set; }
        public int LoseFormationId { get; set; }
        public int DebuffKONum { get; set; }

        public FormationVSFormationDto() {}
    }
}
