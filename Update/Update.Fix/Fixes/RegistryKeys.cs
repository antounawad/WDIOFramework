using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Update.Fix.Fixes
{
    internal class RegistryKeys : FixBase, IFix
    {
        private const string REGISTRY_GROUP_NAME = @"xbAV Beratungssoftware GmbH";
        private static readonly string[] REGISTRY_GROUP_NAME_OBSOLETE = { @"EULG Software GmbH", @"KS Software GmbH" };

        private RegistryKeys() { }

        public static IFix Inst { get; } = new RegistryKeys();

        public string Name => nameof(RegistryKeys);

        public bool? Check()
        {
            var profiles = new[]
            {
                new KeyValuePair<RegistryKey, string>(Registry.CurrentUser, GetHkcuProfileName()),
                new KeyValuePair<RegistryKey, string>(Registry.LocalMachine, GetHklmProfileName())
            };

            foreach (var profile in profiles)
            {
                var key = profile.Key;
                using (var currentKey = key.OpenSubKey($"Software\\{REGISTRY_GROUP_NAME}\\{profile.Value}", RegistryKeyPermissionCheck.ReadSubTree))
                {
                    foreach(var legacyKey in REGISTRY_GROUP_NAME_OBSOLETE)
                    {
                        using (var obsoleteKey = key.OpenSubKey($"Software\\{legacyKey}\\{profile.Value}"))
                        {
                            if(obsoleteKey != null && (currentKey == null || currentKey.SubKeyCount == 0))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public void Apply()
        {
            var hklmProfile = GetHklmProfileName();
            var hkcuProfile = GetHkcuProfileName();

            var profiles = new[]
            {
                new KeyValuePair<RegistryKey, string>(Registry.CurrentUser, hkcuProfile),
                new KeyValuePair<RegistryKey, string>(Registry.LocalMachine, hklmProfile)
            };

            var sourceKeyName = REGISTRY_GROUP_NAME_OBSOLETE.FirstOrDefault(key =>
            {
                return profiles.All(profile => ProfileExists(profile.Key, key, profile.Value));
            });

            string hklmSourceKeyName;
            string hkcuSourceKeyName;

            if (sourceKeyName == null)
            {
                // Kein Registryschlüssel gefunden, der das selbe Profil sowohl in HKLM als auch HKCU enthält. Es gibt aber noch eine Möglichkeit: HKCU+EULG mit HKLM+KS!
                if (REGISTRY_GROUP_NAME_OBSOLETE.Length == 2
                    && ProfileExists(Registry.LocalMachine, REGISTRY_GROUP_NAME_OBSOLETE[1], hklmProfile)
                    && ProfileExists(Registry.CurrentUser, REGISTRY_GROUP_NAME_OBSOLETE[0], hkcuProfile))
                {
                    using (var hklm = Registry.LocalMachine.OpenSubKey($@"Software\{REGISTRY_GROUP_NAME_OBSOLETE[1]}\{hklmProfile}", RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        var installDir = (string)hklm?.GetValue("Install_Dir");
                        var currentDir = GetInstallDir();

                        if (installDir == null || !installDir.TrimEnd('/').Equals(currentDir.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Kann Registry-Quellschlüssel nicht bestimmen (2)");
                        }
                    }

                    hklmSourceKeyName = REGISTRY_GROUP_NAME_OBSOLETE[1];
                    hkcuSourceKeyName = REGISTRY_GROUP_NAME_OBSOLETE[0];
                }
                else
                {
                    throw new Exception("Kann Registry-Quellschlüssel nicht bestimmen (1)");
                }
            }
            else
            {
                hklmSourceKeyName = sourceKeyName;
                hkcuSourceKeyName = sourceKeyName;
            }

            using (var hklmSource = Registry.LocalMachine.OpenSubKey($@"Software\{hklmSourceKeyName}\{hklmProfile}", RegistryKeyPermissionCheck.ReadSubTree))
            {
                using (var hklmDest = Registry.LocalMachine.CreateSubKey($@"Software\{REGISTRY_GROUP_NAME}\{hklmProfile}", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    CopyRegistryKeyRecursively(hklmSource, hklmDest);
                }
            }
            using (var hkcuSource = Registry.CurrentUser.OpenSubKey($@"Software\{hkcuSourceKeyName}\{hkcuProfile}", RegistryKeyPermissionCheck.ReadSubTree))
            {
                using (var hkcuDest = Registry.CurrentUser.CreateSubKey($@"Software\{REGISTRY_GROUP_NAME}\{hkcuProfile}", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    CopyRegistryKeyRecursively(hkcuSource, hkcuDest);
                }
            }

            // Bis jetzt keine Exception geflogen? Dann altes Profil löschen
            using (var hklmParent = Registry.LocalMachine.OpenSubKey($@"Software\{hklmSourceKeyName}", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                hklmParent?.DeleteSubKeyTree(hklmProfile, false);
            }
            using (var hkcuParent = Registry.CurrentUser.OpenSubKey($@"Software\{hkcuSourceKeyName}", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                hkcuParent?.DeleteSubKeyTree(hkcuProfile, false);
            }
        }

        private static bool ProfileExists(RegistryKey registryHive, string companyName, string profileName)
        {
            using (var regkey = registryHive.OpenSubKey($@"Software\{companyName}\{profileName}", RegistryKeyPermissionCheck.ReadSubTree))
            {
                return regkey != null;
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