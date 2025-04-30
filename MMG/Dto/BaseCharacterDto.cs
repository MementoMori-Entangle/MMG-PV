using System;

namespace MMG.Dto
{
    [Serializable]
    public class BaseCharacterDto : Dto
    {
        public int Id { get; set; } = 0;
        public string Attribute { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Speed { get; set; } = 0;
        public int Rarity { get; set; } = 0;
        public int[] SpeedSkillBuff { get; set; }
        public int[] SpeedEWBuff { get; set; }

        public BaseCharacterDto() { }
    }
}
