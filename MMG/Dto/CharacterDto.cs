using System;

namespace MMG.Dto
{
    /*
     レアリティの取り扱い
     0 = N , 1 = R , 2 = R+ , 3 = SR , 4 = SR+ , 5 = SSR , 6 = SSR+ , 7 = UR , 8 = UR+ , 9 = LR , 10 = LR+1 , 11 = LR+2, 12 = LR+3, 13 = LR+4, 14 = LR+5
     */
    [Serializable]
    public class CharacterDto : Dto
    {
        public static readonly String[] RARITY = { "N", "R", "R+", "SR", "SR+", "SSR", "SSR+", "UR", "UR+", "LR", "LR+1", "LR+2", "LR+3", "LR+4", "LR+5", "LR+6", "LR+7", "LR+8", "LR+9", "LR+10" };

        public int Id { get; set; } = 0;
        public int FormationId { get; set; } = 0;
        public long PlayerId { get; set; } = 0;
        public int SortNo { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Attribute { get; set; } = string.Empty;
        public int Rarity { get; set; } = 0;
        public int Level { get; set; } = 0;
        public long ForceValue { get; set; } = 0;
        public int Speed { get; set; } = 0;
        public int SpeedRune1 { get; set; } = 0;
        public int SpeedRune2 { get; set; } = 0;
        public int SpeedRune3 { get; set; } = 0;
        public int PiercingRune1 { get; set; } = 0;
        public int PiercingRune2 { get; set; } = 0;
        public int PiercingRune3 { get; set; } = 0;
        public int LRNum { get; set; } = 0;
        public int URNum { get; set; } = 0;
        public int SSRNum { get; set; } = 0;
        public int SRNum { get; set; } = 0;
        public int RNum { get; set; } = 0;
        public int EWeapon { get; set; } = 0;
        public bool IsAlive { get; set; } = true;
        public bool IsLeft { get; set; } = false;

        public CharacterDto() { }

        public void UpdateFormationId(int addId)
        {
            FormationId += addId * -100;
        }

        public void UpdatePlayerId(long id)
        {
            PlayerId = id;
        }
    }
}
