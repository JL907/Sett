using R2API;
using System;

namespace SettMod.Modules
{
    internal static class Tokens
    {
        public const string bossSubtitle = "The Boss";
        public const string characterLore = "A leader of Ionia's growing criminal underworld, Sett rose to prominence in the wake of the war with Noxus. Though he began as a humble challenger in the fighting pits of Navori, he quickly gained notoriety for his savage strength, and his ability to take seemingly endless amounts of punishment. Now, having climbed through the ranks of local combatants, Sett has muscled to the top, reigning over the pits he once fought in.";
        public const string characterName = "<color=#ffa700>Sett</color>";
        public const string characterOutro = "..and so he left, with newfound might to honor.";
        public const string characterOutroFailure = "..and so he returned, infallible bastion truly immortalized.";
        public const string characterSubtitle = "The Boss";

        public static string descriptionText =
             "Sett is this Boss: a self-made, half-Vastayan, half-Noxian entrepreneur and fighter running Ionia's underground fighting pits.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine
             + "< ! > Sett's basic attacks alternate between left and right punch. Right punch is slightly stronger and faster. Sett also hates losing, gaining additional health regeneration based off of his missing health." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett pulls in all enemies on opposite sides of him, dealing damage and stunning them. If enemies were only on one side, they are slowed instead of stunned." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett carries an enemy champion through the air and slams them into the ground, dealing damage and slowing all enemies near where they land." + Environment.NewLine + Environment.NewLine
             + "< ! > Sett passively stores damage he takes as Grit. On cast, Sett expends all stored Grit to gain a shield and punch an area, dealing true damage in the center and physical damage on the sides." + Environment.NewLine + Environment.NewLine;

        internal static void AddTokens()
        {
            LanguageAPI.Add("SETT_NAME", characterName);
            LanguageAPI.Add("OBSIDIAN_SETT_NAME", "<color=#ffa700>Obsidian Dragon Sett</color>");
            LanguageAPI.Add("POOL_SETT_NAME", "<color=#ffa700>Pool Party Sett</color>");
            LanguageAPI.Add("PRESTIGE_SETT_NAME", "<color=#ffa700>Prestige Obsidian Dragon Sett</color>");
            LanguageAPI.Add("MECHA_SETT_NAME", "<color=#ffa700>Mecha Kingdoms Sett</color>");
            LanguageAPI.Add("SETT_DESCRIPTION", descriptionText);
            LanguageAPI.Add("SETT_SUBTITLE", characterSubtitle);
            LanguageAPI.Add("SETT_LORE", characterLore);
            LanguageAPI.Add("SETT_OUTRO_FLAVOR", characterOutro);
            LanguageAPI.Add("SETT_OUTRO_FAILURE", characterOutroFailure);
            LanguageAPI.Add("SETT_DEFAULT_SKIN_NAME", "<color=#ffa700>Sett</color>");

            LanguageAPI.Add("SETT_PASSIVE_NAME", "<color=#ffa700>HEART OF THE HALF-BEAST</color>");
            LanguageAPI.Add("SETT_PASSIVE_DESC", "Sett <color=#c9aa71>regenerates</color> an additional <color=#008744>0.25 health per second</color> <color=#d62d20>(+ 0.25 every 4 levels)</color> for every <color=#f68835>5%</color> of his <color=#d62d20>missing health.</color>");

            LanguageAPI.Add("SETT_PRIMARY_NAME", "<color=#ffa700>KNUCKLE DOWN</color>");
            LanguageAPI.Add("SETT_PRIMARY_DESC", "Sett's <color=#c9aa71>basic attacks</color> alternate between a Left Punch <color=#f68835>(260% damage)</color> and a Right Punch <color=#f68835>(320% damage)</color>");

            LanguageAPI.Add("SETT_SECONDARY_NAME", "<color=#ffa700>FACEBREAKER</color>");
            LanguageAPI.Add("SETT_SECONDARY_DESC", "Sett <color=#c9aa71>pulls in</color> all enemies within a <color=#0057e7>20 meter</color> radius of him, dealing <color=#f68835>380% </color>damage. <color=#c9aa71>Stun & Slows</color> on hit.");

            LanguageAPI.Add("SETT_UTILITY_NAME", "<color=#ffa700>THE SHOW STOPPER</color>");
            LanguageAPI.Add("SETT_UTILITY_DESC", "Sett <color=#c9aa71>carries</color> an enemy through the air and slams them into the ground, dealing <color=#f68835>1600%</color> <color=#d62d20>(+10% of primary target's maximum health & maximum shield)</color> damage to all enemies near where they land.");

            LanguageAPI.Add("SETT_SPECIAL_NAME", "<color=#ffa700>HAYMAKER</color>");
            LanguageAPI.Add("SETT_SPECIAL_DESC", "Sett passively stores damage he takes as <color=#ffffff>Grit</color>. On cast, Sett expends all stored <color=#ffffff>Grit</color> to gain a <color=#ffffff>Shield</color> and punch an area, dealing <color=#f68835>2100%</color> <color=#d62d20>(+50% of the expended Grit +10% every 4 levels)</color> <color=#ffffff>TRUE</color> damage.");

            LanguageAPI.Add("SETT_CONQUEROR_NAME", "<color=#ffa700>Conqueror</color>");
            LanguageAPI.Add("SETT_CONQUEROR_DESC", "<color=#c9aa71>Successful attacks & abilties</color> against enemies grant <color=#ffffff>1</color> stack of conqueror up to 12 stacks. Each stack of Conqueror grants <color=#f68835>0.6</color> <color=#d62d20>(+0.045 every 4 levels)</color> bonus base damage. While fully stacked you <color=#c9aa71>heal</color> for <color=#008744>6% of any damage from abilities dealt to enemies.</color>");

            LanguageAPI.Add("SETT_LETHAL_NAME", "<color=#ffa700>Lethal Tempo</color>");
            LanguageAPI.Add("SETT_LETHAL_DESC", "<color=#c9aa71>Successful attacks & abilties</color> against enemies grant <color=#ffffff>1</color> stack of lethal tempo up to 6 stacks. Gain <color=#f68835>13%</color> bonus attack speed for each stack up to <color=#f68835>78%</color> bonus attack speed at maximum stacks.");

            LanguageAPI.Add("SETT_PHASE_RUSH_NAME", "<color=#ffa700>Phase Rush</color>");
            LanguageAPI.Add("SETT_PHASE_RUSH_DESC", "<color=#c9aa71>Successful attacks & abilties</color> generate <color=#c9aa71>1</color> stack against enemies. Applying <color=#ffffff>3</color> stacks to a target within a 4 second period grants you <color=#f68835>30%</color> <color=#d62d20>(+5% every 4 levels)</color> bonus <color=#c9aa71>movement speed</color> for 3 seconds. Grants the bonus <color=#c9aa71>movement speed</color> on kill.");

            LanguageAPI.Add("SETT_ELECTROCUTE_NAME", "<color=#ffa700>Electrocute</color>");
            LanguageAPI.Add("SETT_ELECTROCUTE_DESC", "<color=#c9aa71>Successful attacks & abilties</color> generate <color=#c9aa71>1</color> stack against enemies. Applying <color=#ffffff>3</color> stacks to a target within a 3 second period causes them to be struck by lightning after a 1-second delay, dealing them <color=#f68835>600%</color> <color=#d62d20>(+75% every 4 levels)</color> damage. Electrocute has a 5 second cooldown per target.");
        }
    }
}