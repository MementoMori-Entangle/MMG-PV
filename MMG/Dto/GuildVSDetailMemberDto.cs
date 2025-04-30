using System;

namespace MMG.Dto
{
    [Serializable]
    public class GuildVSDetailMemberDto : Dto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int FormationNum { get; set; }
        public int CharacterNum { get; set; }
        public int VirtualityFormation { get; set; }
        public int VFCNum { get; set; }
        public int GBGroupId { get; set; }

        public GuildVSDetailMemberDto() { }
    }
}
