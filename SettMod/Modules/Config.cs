using BepInEx.Configuration;
using System;

namespace SettMod.Modules
{
    public static class Config
    {
        public static ConfigEntry<float> armorGrowth;
        public static ConfigEntry<float> baseArmor;
        public static ConfigEntry<float> baseCrit;
        public static ConfigEntry<float> baseDamage;
        public static ConfigEntry<float> baseHealth;
        public static ConfigEntry<float> baseMovementSpeed;
        public static ConfigEntry<float> baseRegen;
        public static ConfigEntry<float> bonusHealthCoefficient;
        public static ConfigEntry<float> damageGrowth;
        public static ConfigEntry<float> faceBreakerCD;
        public static ConfigEntry<float> faceBreakerDamageCoefficient;
        public static ConfigEntry<float> faceBreakerPullForce;
        public static ConfigEntry<float> faceBreakerPullRadius;
        public static ConfigEntry<float> hayMakerCD;
        public static ConfigEntry<float> hayMakerDamageCoefficient;
        public static ConfigEntry<float> hayMakerGritBonus;
        public static ConfigEntry<float> hayMakerGritBonusPer4;
        public static ConfigEntry<float> healthGrowth;
        public static ConfigEntry<int> jumpCount;
        public static ConfigEntry<float> leftPunchDamageCoefficient;
        public static ConfigEntry<float> regenGrowth;
        public static ConfigEntry<float> rightPunchDamageCoefficient;
        public static ConfigEntry<float> slamCD;
        public static ConfigEntry<float> slamDamageCoefficient;
        public static ConfigEntry<float> slamForce;
        public static ConfigEntry<float> slamRadius;

        public static void ReadConfig()
        {
            baseHealth = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health"), 180f, new ConfigDescription("", null, Array.Empty<object>()));
            healthGrowth = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Growth"), 48f, new ConfigDescription("", null, Array.Empty<object>()));

            baseRegen = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Health Regen"), 1f, new ConfigDescription("", null, Array.Empty<object>()));
            regenGrowth = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Health Regen Growth"), 0.2f, new ConfigDescription("", null, Array.Empty<object>()));

            baseArmor = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Armor"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            armorGrowth = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Armor Growth"), 0f, new ConfigDescription("", null, Array.Empty<object>()));

            baseDamage = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Damage"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
            damageGrowth = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Damage Growth"), 2.8f, new ConfigDescription("", null, Array.Empty<object>()));

            baseMovementSpeed = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Movement Speed"), 8f, new ConfigDescription("", null, Array.Empty<object>()));

            baseCrit = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("01 - Character Stats", "Base Crit"), 1f, new ConfigDescription("", null, Array.Empty<object>()));

            jumpCount = SettPlugin.instance.Config.Bind<int>(new ConfigDefinition("01 - Character Stats", "Jump Count"), 1, new ConfigDescription("", null, Array.Empty<object>()));

            leftPunchDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Kunckle Down", "Kunckle Down Left Punch Damage Coefficient"), 2.6f, new ConfigDescription("", null, Array.Empty<object>()));
            rightPunchDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("02 - Kunckle Down", "Kunckle Down Right Punch Damage Coefficient"), 3.2f, new ConfigDescription("", null, Array.Empty<object>()));

            faceBreakerDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Face Breaker", "Face Breaker Damage Coefficient"), 3.8f, new ConfigDescription("", null, Array.Empty<object>()));
            faceBreakerPullRadius = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Face Breaker", "Face Breaker Pull Radius"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            faceBreakerPullForce = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Face Breaker", "Face Breaker Pull Force"), 200f, new ConfigDescription("", null, Array.Empty<object>()));
            faceBreakerCD = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("03 - Face Breaker", "Face Breaker Cooldown"), 7f, new ConfigDescription("", null, Array.Empty<object>()));

            slamDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Damage Coefficient"), 16f, new ConfigDescription("", null, Array.Empty<object>()));
            bonusHealthCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Bonus Damage Coefficient"), 0.075f, new ConfigDescription("Bonus Damage Based On Primary Target Maximum Health Coefficient", null, Array.Empty<object>()));
            slamRadius = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Slam Radius"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            slamForce = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Slam Force"), 500f, new ConfigDescription("", null, Array.Empty<object>()));
            slamCD = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Cooldown"), 10f, new ConfigDescription("", null, Array.Empty<object>()));

            hayMakerDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Damage Coefficient"), 21f, new ConfigDescription("", null, Array.Empty<object>()));
            hayMakerGritBonus = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Expended Grit Bonus Damage Base Coefficient"), 0.5f, new ConfigDescription("Base Expended Grit Coefficient", null, Array.Empty<object>()));
            hayMakerGritBonusPer4 = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Additional Bonus Damage Coefficient Per 4 Levels"), 0.1f, new ConfigDescription("Additional Expended Grit Coefficient Per 4 Levels", null, Array.Empty<object>()));
            hayMakerCD = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Cooldown"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
        }

        // this helper automatically makes config entries for disabling survivors
        internal static ConfigEntry<bool> CharacterEnableConfig(string characterName)
        {
            return SettPlugin.instance.Config.Bind<bool>(new ConfigDefinition(characterName, "Enabled"), true, new ConfigDescription("Set to false to disable this character"));
        }

        internal static ConfigEntry<bool> EnemyEnableConfig(string characterName)
        {
            return SettPlugin.instance.Config.Bind<bool>(new ConfigDefinition(characterName, "Enabled"), true, new ConfigDescription("Set to false to disable this enemy"));
        }
    }
}