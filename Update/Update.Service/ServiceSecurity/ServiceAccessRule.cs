using System.Security.AccessControl;
using System.Security.Principal;

namespace Eulg.Update.Service.ServiceSecurity
{
    public sealed class ServiceAccessRule : AccessRule
    {
        // Methods
        public ServiceAccessRule(IdentityReference identity,
            ServiceRights serviceRights,
            AccessControlType accessControlType)
            : this(identity,
            (int)serviceRights,
            false,
            InheritanceFlags.None,
            PropagationFlags.None,
            accessControlType)
        {
        }

        public ServiceAccessRule(string identity,
            ServiceRights serviceRights,
            AccessControlType accessControlType)
            : this(new NTAccount(identity),
            (int)serviceRights,
            false,
            InheritanceFlags.None,
            PropagationFlags.None,
            accessControlType)
        {
        }

        internal ServiceAccessRule(IdentityReference identity,
            int accessMask,
            bool isInherited,
            InheritanceFlags inheritanceFlags,
            PropagationFlags propagationFlags,
            AccessControlType accessControlType)
            : base(identity,
            accessMask,
            isInherited,
            inheritanceFlags,
            propagationFlags,
            accessControlType)
        {
        }

        // Properties
        public ServiceRights ServiceRights
        {
            get
            {
                return (ServiceRights)base.AccessMask;
            }
        }
    }
}
