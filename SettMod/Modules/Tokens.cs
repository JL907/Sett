using R2API;
using System;

namespace SettMod.Modules
{
    internal static class Tokens
    {
        public const string characterLore = "A leader of Ionia's growing criminal underworld, Sett rose to prominence in the wake of the war with Noxus. Though he began as a humble challenger in the fighting pits of Navori, he quickly gained notoriety for his savage strength, and his ability to take seemingly endless amounts of punishment. Now, having climbed through the ranks of local combatants, Sett has muscled to the top, reigning over the pits he once fought in.";
        public const string characterName = "<color=#ffa700>Sett</color>";
        public const string characterOutro = "..and so he left, with newfound might to honor.";
        public const string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
        public const string characterSubtitle = "The Boss";
        internal static string descriptionText = "Sett is this Boss: a self-made, half-Vastayan, half-Noxian entrepreneur and fighter running Ionia's underground fighting pits.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
                                                                                     + "< ! > Sett's basic attacks alternate between left and right punch. Right punch is slightly stronger and faster. Sett also hates losing, gaining additional health regeneration based off of his missing health." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett pulls in all enemies on opposite sides of him, dealing damage and stunning them. If enemies were only on one side, they are slowed instead of stunned." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett carries an enemy champion through the air and slams them into the ground, dealing damage and slowing all enemies near where they land." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett passively stores damage he takes as Grit. On cast, Sett expends all stored Grit to gain a shield and punch an area, dealing true damage in the center and physical damage on the sides." + Environment.NewLine + Environment.NewLine;

        public const string defaultSkinName = "Original";

        public const string primaryName = "<color=#ffa700>KNUCKLE DOWN</color>";
        public const string primaryDesc = "Sett's <color=#c9aa71>basic attacks</color> alternate between a Left Punch <color=#f68835>(280% damage)</color> and a Right Punch <color=#f68835>(360% damage)</color>";

        public const string secondaryName = "<color=#ffa700>FACEBREAKER</color>";
        public const string secondaryDesc = "Sett <color=#c9aa71>pulls in</color> all enemies within a <color=#0057e7>20 unit</color> radius of him, dealing <color=#f68835>400% </color>damage. <color=#c9aa71>Stun & Slows</color> on hit.";

        public const string utilityName = "<color=#ffa700>THE SHOW STOPPER</color>";
        public const string utilityDesc = "Sett <color=#c9aa71>carries</color> an enemy through the air and slams them into the ground, dealing <color=#f68835>1200%</color> <color=#d62d20>(+5% of primary target's total health)</color> damage to all enemies near where they land.";

        public const string specialName = "<color=#ffa700>HAYMAKER</color>";
        public const string specialDesc = "Sett passively stores damage he takes as <color=#ffffff>Grit</color>. On cast, Sett expends all stored <color=#ffffff>Grit</color> to gain a <color=#ffffff>Shield</color> and punch an area, dealing <color=#f68835>1400%</color> <color=#d62d20>(+300% of the expended Grit)</color> <color=#ffffff>TRUE</color> damage.";

        internal static void AddTokens()
        {
            LanguageAPI.Add("SETT_NAME", characterName);
            LanguageAPI.Add("SETT_DESCRIPTION", descriptionText);
            LanguageAPI.Add("SETT_SUBTITLE", characterSubtitle);
            LanguageAPI.Add("SETT_LORE", characterLore);
            LanguageAPI.Add("SETT_OUTRO_FLAVOR", characterOutro);
            LanguageAPI.Add("SETT_OUTRO_FAILURE", characterOutroFailure);

            LanguageAPI.Add("PRIMARY_NAME", primaryName);
            LanguageAPI.Add("PRIMARY_DESC", primaryDesc);

            LanguageAPI.Add("SECONDARY_NAME", secondaryName);
            LanguageAPI.Add("SECONDARY_DESC", secondaryDesc);

            LanguageAPI.Add("SETT_UTILITY_NAME", utilityName);
            LanguageAPI.Add("SETT_UTILITY_DESC", utilityDesc);

            LanguageAPI.Add("SETT_SPECIAL_NAME", utilityName);
            LanguageAPI.Add("SETT_SPECIAL_DESC", utilityDesc);

            LanguageAPI.Add("SETT_DEFAULT_SKIN_NAME", defaultSkinName);


        }
    }
}