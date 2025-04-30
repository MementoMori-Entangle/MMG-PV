using System;
using System.Collections.Generic;

namespace MMG.Dto
{
    public class GuildBattleProgressDto : Dto
    {
        public long MMId { get; set; }
        public string Name { get; set; }
        public string Stamina { get; set; }
        public Dictionary<string, int> Formation { get; set; }
        public Dictionary<string, int> FormationIdName { get; set; }
        public bool IsOnline { get; set; } = true;
        public DateTime CreatedAt { get; set; } = new DateTime();
    }
}
