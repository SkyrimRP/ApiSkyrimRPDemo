using System;

namespace Domain.Services.JwtAuthManager.Models
{
    public class RefreshToken
    {
        public int UID { get; set; }
        public string TokenString { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
