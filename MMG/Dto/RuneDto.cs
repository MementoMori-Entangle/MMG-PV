using System;

namespace MMG.Dto
{
    [Serializable]
    public class RuneDto : Dto
    {
        public static int[] Speed { get; set; } = { 0, 10, 18, 33, 53, 80, 110, 150, 195, 240, 300, 360, 425, 500, 575, 600 };

        public RuneDto() { }
    }
}
