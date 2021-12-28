using System;

namespace Domain.Enums
{
    [Flags]
    public enum ServerFlags : ushort
    {
        None,
        Reserved = 0x0001,
        IsOfficial = 0x0002,
        HasSkyEye = 0x0004,
        HasWhitelist = 0x0008,
        HasPassword = 0x0010
    }
}
