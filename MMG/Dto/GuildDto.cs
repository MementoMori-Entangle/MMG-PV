using System;

namespace MMG.Dto
{
    [Serializable]
    public class GuildDto : Dto
    {
        public int Id { get; set; } = 0;
        public int No { get; set; } = 0;
        public int World { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public int MemberNum { get; set; } = 0;
        public int Stamina { get; set; } = 0;
        public long ForceValue { get; set; } = 0;
        public string Remarks { get; set; } = string.Empty;
        public PlayerDto[] Members { get; set; } = new PlayerDto[0];
        public bool IsDelete { get; set; } = false;
        public GuildDto() { }
    }
}
