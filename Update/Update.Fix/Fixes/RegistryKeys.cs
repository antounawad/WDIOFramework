﻿using Microsoft.Win32;

namespace Update.Fix.Fixes
{
    internal static class RegistryKeys
    {
        private const string REGISTRY_GROUP_NAME = @"Software\EULG Software GmbH";
        private const string REGISTRY_GROUP_NAME_OBSOLETE = @"Software\KS Software GmbH";

        internal static bool Check()
        {
            try
            {
                foreach (var key in new[] { Registry.LocalMachine, Registry.CurrentUser })
                {
                    var obsoleteKey = key.OpenSubKey(REGISTRY_GROUP_NAME_OBSOLETE);
                    var currentKey = key.OpenSubKey(REGISTRY_GROUP_NAME);
                    if (obsoleteKey != null && (currentKey == null || currentKey.SubKeyCount == 0))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void Fix()
        {
            foreach (var key in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                var obsoleteKey = key.OpenSubKey(REGISTRY_GROUP_NAME_OBSOLETE);
                var currentKey = key.OpenSubKey(REGISTRY_GROUP_NAME, RegistryKeyPermissionCheck.ReadWriteSubTree) ?? key.CreateSubKey(REGISTRY_GROUP_NAME, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (obsoleteKey != null)
                {
                    CopyRegistryKeyRecursively(obsoleteKey, currentKey);
                    key.DeleteSubKeyTree(REGISTRY_GROUP_NAME_OBSOLETE, false);
                }
            }
        }

        private static void CopyRegistryKeyRecursively(RegistryKey source, RegistryKey destination)
        {
            foreach (var valName in source.GetValueNames())
            {
                destination.SetValue(valName, source.GetValue(valName), source.GetValueKind(valName));
            }
            foreach (var subkey in source.GetSubKeyNames())
            {
                CopyRegistryKeyRecursively(source.OpenSubKey(subkey), destination.OpenSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree) ?? destination.CreateSubKey(subkey));
            }
        }
    }
}