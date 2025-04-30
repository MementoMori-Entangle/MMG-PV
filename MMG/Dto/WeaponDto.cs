using System;

namespace MMG.Dto
{
    [Serializable]
    public class WeaponDto : Dto
    {
        public static readonly string[] EXCLUSIVE_WEAPON = { "", "SSR", "UR", "LR" };

        public WeaponDto() { }
    }
}
