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

        public static ConfigEntry<float> heartBeast;
        public static ConfigEntry<float> heartBeastPer4;

        public static ConfigEntry<float> conquerorDamageStack;
        public static ConfigEntry<float> conquerorDamageStackPer4;

        public static ConfigEntry<float> lethalAttackSpeedStack;

        public static ConfigEntry<float> phaseMovementSpeed;
        public static ConfigEntry<float> phaseMovementSpeedPer4;

        public static ConfigEntry<float> electrocuteDamage;
        public static ConfigEntry<float> electrocuteDamagePer4;

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

            slamDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Damage Coefficient"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
            bonusHealthCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Bonus Damage Coefficient"), 0.05f, new ConfigDescription("Bonus Damage Based On Primary Target Maximum Health & Maximum Shield Coefficient", null, Array.Empty<object>()));
            slamRadius = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Slam Radius"), 20f, new ConfigDescription("", null, Array.Empty<object>()));
            slamForce = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Slam Force"), 500f, new ConfigDescription("", null, Array.Empty<object>()));
            slamCD = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("04 - The Show Stopper", "The Show Stopper Cooldown"), 10f, new ConfigDescription("", null, Array.Empty<object>()));

            hayMakerDamageCoefficient = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Damage Coefficient"), 16f, new ConfigDescription("", null, Array.Empty<object>()));
            hayMakerGritBonus = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Expended Grit Bonus Damage Base Coefficient"), 0.5f, new ConfigDescription("Base Expended Grit Coefficient", null, Array.Empty<object>()));
            hayMakerGritBonusPer4 = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Additional Bonus Damage Coefficient Per 4 Levels"), 0.025f, new ConfigDescription("Additional Expended Grit Coefficient Per 4 Levels", null, Array.Empty<object>()));
            hayMakerCD = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("05 - Haymaker", "Haymaker Cooldown"), 12f, new ConfigDescription("", null, Array.Empty<object>()));
            
            heartBeast = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("06 - Passive", "Heart of the Half Beast Regen Coefficient"), 0.25f, new ConfigDescription("", null, Array.Empty<object>()));
            heartBeastPer4 = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("06 - Passive", "Heart of the Half Beast Regen Additional Coefficient per 4 Levels"), 0.25f, new ConfigDescription("", null, Array.Empty<object>()));

            conquerorDamageStack = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("07 - Conqueror", "Conqueror base damage per stack"), 0.6f, new ConfigDescription("", null, Array.Empty<object>()));
            conquerorDamageStack = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("07 - Conqueror", "Conqueror additional base damage per 4 levels"), 0.045f, new ConfigDescription("", null, Array.Empty<object>()));

            lethalAttackSpeedStack = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("08 - Lethal Tempo", "Lethal Tempo attack speed per stack"), 0.1f, new ConfigDescription("", null, Array.Empty<object>()));

            phaseMovementSpeed = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("09 - Phase Rush", "Phase Rush Movement Speed %"), 0.3f, new ConfigDescription("", null, Array.Empty<object>()));
            phaseMovementSpeedPer4 = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("09 - Phase Rush", "Phase Rush Additional Movement Speed % per 4 levels"), 0.05f, new ConfigDescription("", null, Array.Empty<object>()));

            electrocuteDamage = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("10 - Electrocute", "Electrocute base damage coefficient"), 6f, new ConfigDescription("", null, Array.Empty<object>()));
            electrocuteDamagePer4 = SettPlugin.instance.Config.Bind<float>(new ConfigDefinition("10 - Electrocute", "Electrocute additional coefficient per 4 levels"), 0.75f, new ConfigDescription("", null, Array.Empty<object>()));


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