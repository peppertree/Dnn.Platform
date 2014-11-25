using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Security.Authentication
{
    public enum AuthenticationStatus
    {
        Authenticated = 10,
        InvalidCredentials = 20,
        InvalidPassword = 21,
        InvalidUsername =22,
        InvalidEmail = 23,
        LockedOut = 30,
        NotApproved = 40,
        NeedsPasswordChange = 50,
        MethodNotSupported = 60
    }
}
