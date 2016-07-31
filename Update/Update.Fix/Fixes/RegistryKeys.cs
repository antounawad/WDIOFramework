using System.Linq;
using Microsoft.Win32;

namespace Update.Fix.Fixes
{
    internal class RegistryKeys : IFix
    {
        private const string REGISTRY_GROUP_NAME = @"Software\xbAV Beratungssoftware GmbH";
        private static string[] REGISTRY_GROUP_NAME_OBSOLETE = { @"Software\EULG Software GmbH", @"Software\KS Software GmbH" };

        private RegistryKeys() { }

        public static IFix Inst { get; } = new RegistryKeys();

        public string Name => nameof(RegistryKeys);

        public bool? Check()
        {
            //FIXME Nur das aktuelle Profil bearbeiten sonst werden parallele 1.x und 2.x Installationen zerstört
            foreach (var key in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                foreach(var legacyKey in REGISTRY_GROUP_NAME_OBSOLETE)
                {
                    var obsoleteKey = key.OpenSubKey(legacyKey);
                    var currentKey = key.OpenSubKey(REGISTRY_GROUP_NAME, RegistryKeyPermissionCheck.ReadSubTree);

                    if(obsoleteKey != null && (currentKey == null || currentKey.SubKeyCount == 0))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        //FIXME Transaktional arbeiten: DeleteSubKeyTree() erst dann ausführen, wenn Migrationsvorgang bei allen Keys erfolgreich
        public void Apply()
        {
            foreach (var key in new[] { Registry.LocalMachine, Registry.CurrentUser })
            {
                var obsoleteKeyName = REGISTRY_GROUP_NAME_OBSOLETE.FirstOrDefault(k => key.OpenSubKey(k) != null);

                if (obsoleteKeyName != null)
                {
                    var obsoleteKey = key.OpenSubKey(obsoleteKeyName);
                    var currentKey = key.OpenSubKey(REGISTRY_GROUP_NAME, RegistryKeyPermissionCheck.ReadWriteSubTree) ?? key.CreateSubKey(REGISTRY_GROUP_NAME, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if(obsoleteKey != null)
                    {
                        CopyRegistryKeyRecursively(obsoleteKey, currentKey);
                        key.DeleteSubKeyTree(obsoleteKeyName, false);
                    }
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