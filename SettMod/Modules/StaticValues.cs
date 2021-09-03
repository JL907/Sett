using System;

namespace SettMod.Modules
{
    internal static class StaticValues
    {
        internal static string descriptionText = "Sett is a skilled fighter who makes use of a wide arsenal of weaponry to take down his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sett's basic attacks alternate between left and right punch. Right punch is slightly stronger and faster. Sett also hates losing, gaining additional health regeneration based off of his missing health." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett pulls in all enemies on opposite sides of him, dealing damage and stunning them. If enemies were only on one side, they are slowed instead of stunned." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett carries an enemy champion through the air and slams them into the ground, dealing damage and slowing all enemies near where they land." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett passively stores damage he takes as Grit. On cast, Sett expends all stored Grit to gain a shield and punch an area, dealing true damage in the center and physical damage on the sides." + Environment.NewLine + Environment.NewLine;

        internal const float swordDamageCoefficient = 3.8f;

        internal const float gunDamageCoefficient = 4.2f;

        internal const float bombDamageCoefficient = 16f;

        public const string characterName = "Sett";
        public const string characterSubtitle = "The Boss";
        public const string bossSubtitle = "The Boss";
        public const string characterOutro = "..and so he left, with newfound might to honor.";
        public const string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
        public const string characterLore = "A leader of Ionia's growing criminal underworld, Sett rose to prominence in the wake of the war with Noxus. Though he began as a humble challenger in the fighting pits of Navori, he quickly gained notoriety for his savage strength, and his ability to take seemingly endless amounts of punishment. Now, having climbed through the ranks of local combatants, Sett has muscled to the top, reigning over the pits he once fought in.";

    }
}