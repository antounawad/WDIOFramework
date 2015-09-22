using System.Security.AccessControl;
using System.Security.Principal;

namespace Eulg.Update.Service.ServiceSecurity
{
    public sealed class ServiceAuditRule : AuditRule
    {
        // Methods
        public ServiceAuditRule(IdentityReference identity,
            ServiceRights serviceRights,
            AuditFlags auditFlags)
            : this(identity,
            (int)serviceRights,
            false,
            InheritanceFlags.None,
            PropagationFlags.None,
            auditFlags)
        {
        }

        internal ServiceAuditRule(IdentityReference identity,
            int accessMask,
            bool isInherited,
            InheritanceFlags inheritanceFlags,
            PropagationFlags propagationFlags,
            AuditFlags auditFlags)
            : base(identity,
            accessMask,
            isInherited,
            inheritanceFlags,
            propagationFlags,
            auditFlags)
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
