using BepInEx.Configuration;

namespace Keystones
{
    internal static class KeystoneConfig
    {
        internal static ConfigFile file;

        // General
        internal static ConfigEntry<bool> allSurvivors;

        // Conqueror
        internal static ConfigEntry<int> conquerorMaxStacks;
        internal static ConfigEntry<float> conquerorDuration;
        internal static ConfigEntry<float> conquerorDamagePerStack;
        internal static ConfigEntry<float> conquerorDamagePerStackPerLevelStep;
        internal static ConfigEntry<float> conquerorHealFraction;

        // Lethal Tempo
        internal static ConfigEntry<int> lethalMaxStacks;
        internal static ConfigEntry<float> lethalDuration;
        internal static ConfigEntry<float> lethalAttackSpeedPerStack;

        // Phase Rush
        internal static ConfigEntry<float> phaseRushWindow;
        internal static ConfigEntry<float> phaseRushSpeedDuration;
        internal static ConfigEntry<float> phaseRushSpeed;
        internal static ConfigEntry<float> phaseRushSpeedPerLevelStep;

        // Electrocute
        internal static ConfigEntry<float> electrocuteWindow;
        internal static ConfigEntry<float> electrocuteCooldown;
        internal static ConfigEntry<float> electrocuteDamage;
        internal static ConfigEntry<float> electrocuteDamagePerLevelStep;

        internal static void Init(ConfigFile config)
        {
            file = config;

            allSurvivors = config.Bind("General", "Enable For All Survivors", true,
                "Give every survivor a Keystone loadout slot. When false, only survivors whose mod registers them through KeystoneAPI get one. " +
                "Individual survivors can still be switched off in the [Survivors] section, which fills in after the first launch.");

            conquerorMaxStacks = config.Bind("Conqueror", "Max Stacks", 12, "");
            conquerorDuration = config.Bind("Conqueror", "Stack Duration", 4f, "Seconds without hitting anything before stacks start falling off.");
            conquerorDamagePerStack = config.Bind("Conqueror", "Base Damage Per Stack", 0.6f, "Flat base damage added per stack.");
            conquerorDamagePerStackPerLevelStep = config.Bind("Conqueror", "Base Damage Per Stack Per 4 Levels", 0.045f, "");
            conquerorHealFraction = config.Bind("Conqueror", "Heal Fraction At Max Stacks", 0.06f, "Fraction of damage dealt returned as healing while fully stacked.");

            lethalMaxStacks = config.Bind("Lethal Tempo", "Max Stacks", 6, "");
            lethalDuration = config.Bind("Lethal Tempo", "Stack Duration", 6f, "Seconds without hitting anything before stacks start falling off.");
            lethalAttackSpeedPerStack = config.Bind("Lethal Tempo", "Attack Speed Per Stack", 0.10f, "Fraction of attack speed added per stack.");

            phaseRushWindow = config.Bind("Phase Rush", "Stack Window", 4f, "Seconds each Phase Rush stack lasts on a target.");
            phaseRushSpeedDuration = config.Bind("Phase Rush", "Speed Duration", 3f, "");
            phaseRushSpeed = config.Bind("Phase Rush", "Movement Speed", 0.30f, "Fraction of movement speed added.");
            phaseRushSpeedPerLevelStep = config.Bind("Phase Rush", "Movement Speed Per 4 Levels", 0.05f, "");

            electrocuteWindow = config.Bind("Electrocute", "Stack Window", 3f, "Seconds each Electrocute stack lasts on a target.");
            electrocuteCooldown = config.Bind("Electrocute", "Cooldown Per Target", 5f, "");
            electrocuteDamage = config.Bind("Electrocute", "Damage Coefficient", 6f, "Lightning damage as a multiple of your damage stat.");
            electrocuteDamagePerLevelStep = config.Bind("Electrocute", "Damage Coefficient Per 4 Levels", 0.75f, "");
        }

        /// <summary>Per-survivor toggle, created on demand once survivors are known.</summary>
        internal static ConfigEntry<bool> BindSurvivor(string bodyName, string displayName)
        {
            return file.Bind("Survivors", Sanitize(bodyName), true, "Give " + displayName + " a Keystone slot.");
        }

        private static string Sanitize(string key)
        {
            // BepInEx rejects these characters in keys.
            foreach (char c in new[] { '=', '\n', '\t', '\\', '"', '\'', '[', ']' })
                key = key.Replace(c, '_');
            return key;
        }
    }
}
