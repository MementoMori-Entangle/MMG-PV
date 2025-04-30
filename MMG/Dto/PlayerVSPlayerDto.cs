using System;

namespace MMG.Dto
{
    [Serializable]
    public class PlayerVSPlayerDto : Dto
    {
        public long UId { get; set; }
        public long WinPlayerId { get; set; }
        public long LosePlayerId { get; set; }

        public PlayerVSPlayerDto() { }
    }
}
