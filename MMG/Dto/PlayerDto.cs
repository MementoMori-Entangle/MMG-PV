using System;

namespace MMG.Dto
{
    [Serializable]
    public class PlayerDto : Dto
    {
        public int UId { get; set; } = 0;
        public long Id { get; set; } = 0;
        public int No { get; set; } = 0;
        public int World { get; set; } = 0;
        public int GuildId { get; set; } = 0;
        public string GuildName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Stamina { get; set; } = 0;
        public long ForceValue { get; set; } = 0;
        public string MainFormation { get; set; } = string.Empty;
        public FormationDto[] Formations { get; set; } = new FormationDto[0];
        public bool IsOpenWindow { get; set; } = false;
        public bool IsBattle { get; set; } = false;
        public bool IsOnline { get; set; } = false;
        public bool IsVC { get; set; } = false;
        public int VirtualityFormation { get; set; } = 0;
        public int VFCNum { get; set; } = 0;
        public int GBGroupId { get; set; } = 100;
        public bool IsDelete { get; set; } = false;
        public PlayerDto() { }
    }
}
