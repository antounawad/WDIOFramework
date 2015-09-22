using System;

namespace Eulg.Update.Service.ServiceSecurity
{
    [Flags]
    public enum ServiceRights
    {
        // general permissions
        Read = 0x0002008D,
        Write = 0x00020002,
        Execute = 0x00020170,
        FullControl = 0x000F01FF,

        // specific permissions
        QueryConfiguration = 0x0001,
        ChangeConfiguration = 0x0002,
        QueryStatus = 0x0004,
        EnumerateDependents = 0x0008,
        Start = 0x0010,
        Stop = 0x0020,
        PauseOrContinue = 0x0040,
        Interrogate = 0x0080,
        SendUserDefinedControl = 0x0100,

        Delete = 0x00010000,
        ReadPermission = 0x00020000,
        ChangePermission = 0x00040000,
        TakeOwnership = 0x00080000
    }
}
