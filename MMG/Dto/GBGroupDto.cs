using System.Windows;

namespace MMG.Dto
{
    public class GBGroupDto : Dto
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public int No { get; set; }
        public int CharacterNum { get; set; }
        public Rect BaseRect { get; set; }
        public string Name { get; set; }

        public GBGroupDto() { }
    }
}
