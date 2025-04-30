using System;

namespace MMG.Dto
{
    [Serializable]
    public class FormationDto : Dto
    {
        public int Id { get; set; } = 0;
        public long PlayerId { get; set; } = 0;
        public int SortNo { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CharacterDto[] Characters { get; set; } = new CharacterDto[0];
        public bool IsBattle { get; set; } = false;
        public bool IsSubParty { get; set; } = false;
        public bool IsDebuff { get; set; } = false;
        public bool IsGVS { get; set; } = true;

        public FormationDto() { }

        public void UpdateId(int addId)
        {
            Id += addId * -100;
        }

        public void RestorationId(int delId)
        {
            Id += delId * 100;
        }

        public void UpdatePlayerId(long id)
        {
            PlayerId = id;
        }
    }
}
