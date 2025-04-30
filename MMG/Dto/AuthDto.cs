using System;

namespace MMG.Dto
{
    [Serializable]
    public class AuthDto : Dto
    {
        public string LoginId { get; set; }
        public string Password { get; set; }
        public AuthDto() { }
    }
}
