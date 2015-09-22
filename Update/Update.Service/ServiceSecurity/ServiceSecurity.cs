using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Eulg.Update.Service.ServiceSecurity
{
    public sealed class ServiceSecurity : NativeObjectSecurity
    {
        public ServiceSecurity()
            : base(true, ResourceType.Service)
        {
        }

        public ServiceSecurity(string serviceName, AccessControlSections includeSections)
            : base(true, ResourceType.Service, serviceName, includeSections, null, null)
        {
        }

        public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType accessControlType)
        {
            return new ServiceAccessRule(identityReference,
                accessMask,
                isInherited,
                inheritanceFlags,
                propagationFlags,
                accessControlType);
        }
        public void AddAccessRule(ServiceAccessRule rule)
        {
            base.AddAccessRule(rule);
        }
        public void AddAuditRule(ServiceAuditRule rule)
        {
            base.AddAuditRule(rule);
        }
        public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
        {
            return new ServiceAuditRule(identityReference,
                accessMask,
                isInherited,
                inheritanceFlags,
                propagationFlags,
                auditFlags);
        }
        internal AccessControlSections GetAccessControlSectionsFromChanges()
        {
            AccessControlSections sections = AccessControlSections.None;
            if (base.AccessRulesModified)
            {
                sections = AccessControlSections.Access;
            }
            if (base.AuditRulesModified)
            {
                sections |= AccessControlSections.Audit;
            }
            if (base.OwnerModified)
            {
                sections |= AccessControlSections.Owner;
            }
            if (base.GroupModified)
            {
                sections |= AccessControlSections.Group;
            }
            return sections;

        }
        internal void Persist(string serviceName)
        {
            base.WriteLock();
            try
            {
                bool flag1;
                bool flag2;
                AccessControlSections sections = this.GetAccessControlSectionsFromChanges();
                if (sections == AccessControlSections.None)
                {
                    return;
                }
                base.Persist(serviceName, sections);
                base.AccessRulesModified = flag1 = false;
                base.AuditRulesModified = flag2 = flag1;
                base.OwnerModified = base.GroupModified = flag2;
            }
            finally
            {
                base.WriteUnlock();
            }
        }
        public bool RemoveAccessRule(ServiceAccessRule rule)
        {
            return base.RemoveAccessRule(rule);
        }
        public void RemoveAccessRuleAll(ServiceAccessRule rule)
        {
            base.RemoveAccessRuleAll(rule);
        }
        public void RemoveAccessRuleSpecific(ServiceAccessRule rule)
        {
            base.RemoveAccessRuleSpecific(rule);
        }
        public bool RemoveAuditRule(ServiceAuditRule rule)
        {
            return base.RemoveAuditRule(rule);
        }
        public void RemoveAuditRuleAll(ServiceAuditRule rule)
        {
            base.RemoveAuditRuleAll(rule);
        }
        public void RemoveAuditRuleSpecific(ServiceAuditRule rule)
        {
            base.RemoveAuditRuleSpecific(rule);
        }
        public void ResetAccessRule(ServiceAccessRule rule)
        {
            base.ResetAccessRule(rule);
        }
        public void SetAccessRule(ServiceAccessRule rule)
        {
            base.SetAccessRule(rule);
        }
        public void SetAuditRule(ServiceAuditRule rule)
        {
            base.SetAuditRule(rule);
        }

        // Properties
        public override Type AccessRightType
        {
            get
            {
                return typeof(ServiceRights);
            }
        }
        public override Type AccessRuleType
        {
            get
            {
                return typeof(ServiceAccessRule);
            }
        }
        public override Type AuditRuleType
        {
            get
            {
                return typeof(ServiceAuditRule);
            }
        }
    }
}
