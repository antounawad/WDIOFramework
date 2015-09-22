using System;
using System.Security.AccessControl;

namespace Eulg.Update.Service.ServiceSecurity
{
    public class Service
    {
        public static void SetAccessControl(string serviceName, ServiceSecurity serviceSecurity)
        {
            if (serviceSecurity == null)
            {
                throw new ArgumentNullException("serviceSecurity");
            }

            serviceSecurity.Persist(serviceName);
        }

        public static ServiceSecurity GetAccessControl(string serviceName)
        {
            return Service.GetAccessControl(serviceName, AccessControlSections.All);
        }

        public static ServiceSecurity GetAccessControl(string serviceName, AccessControlSections includeSections)
        {
            return new ServiceSecurity(serviceName, includeSections);
        }
    }
}
