using System;
using System.Collections.Generic;
using System.Text;

namespace System.DirectoryServices.Protocols.Services
{
    [Flags]
    public enum AuthenticationTypes
    {
        None = 0,
        Secure = 0x1

    }
}
