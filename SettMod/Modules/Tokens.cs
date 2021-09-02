using R2API;

namespace SettMod.Modules
{
    internal static class Tokens
    {
        internal static void AddTokens()
        {
            string prefix = SettPlugin.developerPrefix + "_SETT_BODY_";

            string outro = "..and so he left, bottom text.";

            LanguageAPI.Add(prefix + "Name", "Sett");
            LanguageAPI.Add(prefix + "DESCRIPTION", Modules.StaticValues.descriptionText);
            LanguageAPI.Add(prefix + "OUTRO_FLAVOR", outro);
        }
    }
}