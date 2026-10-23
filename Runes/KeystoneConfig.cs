using BepInEx.Configuration;

namespace Runes
{
    internal static class KeystoneConfig
    {
        internal static ConfigFile file;

        internal static ConfigEntry<bool> allSurvivors;

        internal static void Init(ConfigFile config)
        {
            file = config;

            allSurvivors = config.Bind("General", "Enable For All Survivors", true,
                "Give every survivor a Keystone loadout slot. When false, only survivors whose mod registers them through KeystoneAPI get one. " +
                "Individual survivors can still be switched off in the [Survivors] section, which fills in after the first launch.");

            foreach (Keystone keystone in KeystoneContent.keystones)
            {
                keystone.BindConfig(config);
            }
        }

        internal static ConfigEntry<bool> BindSurvivor(string bodyName, string displayName)
        {
            return file.Bind("Survivors", Sanitize(bodyName), true, "Give " + displayName + " a Keystone slot.");
        }

        private static string Sanitize(string key)
        {
            foreach (char c in new[] { '=', '\n', '\t', '\\', '"', '\'', '[', ']' })
                key = key.Replace(c, '_');
            return key;
        }
    }
}
