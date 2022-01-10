using BepInEx.Configuration;
using UnityEngine;

namespace SettMod.Modules.Survivors
{
    internal class Obsidian : Sett
    {
        internal static Material obsidianMat = Modules.Assets.CreateMaterial("settObsidianMat");

        internal override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            bodyName = "ObsidianBody",
            bodyNameToken = "OBSIDIAN_SETT_NAME",
            bodyColor = new Color(.3781f, .1324f, .4894f),
            characterPortrait = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("obsidian_square"),
            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            subtitleNameToken = "SETT_NAME_SUBTITLE",
            podPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = Modules.Config.baseHealth.Value,
            healthGrowth = Modules.Config.healthGrowth.Value,

            healthRegen = Modules.Config.baseRegen.Value,
            regenGrowth = Modules.Config.regenGrowth.Value,

            moveSpeed = Modules.Config.baseMovementSpeed.Value,

            damage = Modules.Config.baseDamage.Value,
            damageGrowth = Modules.Config.damageGrowth.Value,

            armor = Modules.Config.baseArmor.Value,
            armorGrowth = Modules.Config.armorGrowth.Value,

            crit = Modules.Config.baseCrit.Value,

            jumpCount = Modules.Config.jumpCount.Value
        };

        internal override string bodyName { get; set; } = "Obsidian";
        internal override GameObject bodyPrefab { get; set; }
        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = obsidianMat,
                }};

        internal override GameObject displayPrefab { get; set; }
        internal override int mainRendererIndex { get; set; } = 0;
        internal override float sortPosition { get; set; } = 1f;

        internal override void InitializeCharacter()
        {
            characterEnabled = SettPlugin.instance.Config.Bind<bool>(new ConfigDefinition(bodyName, "Enabled"), false, new ConfigDescription("Set to false to disable this character"));
            base.InitializeCharacter();
        }

        internal override void InitializeDoppelganger()
        {
            base.InitializeDoppelganger();
        }

        internal override void InitializeSkills()
        {
            base.InitializeSkills();
        }

        internal override void InitializeSkins()
        {
            base.InitializeSkins();
        }

        internal override void InitializeUnlockables()
        {
        }
    }
}