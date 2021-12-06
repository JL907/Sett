using BepInEx.Configuration;
using UnityEngine;

namespace SettMod.Modules.Survivors
{
    internal class Prestige : Sett
    {
        internal static Material prestigeMat = Modules.Assets.CreateMaterial("skin9_0");

        internal override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            bodyName = "PrestigeBody",
            bodyNameToken = "PRESTIGE_SETT_NAME",
            bodyColor = new Color(.3781f, .1324f, .4894f),
            characterPortrait = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("prestige_square"),
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

        internal override string bodyName { get; set; } = "Prestige";
        internal override GameObject bodyPrefab { get; set; }
        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = prestigeMat,
                }};

        internal override GameObject displayPrefab { get; set; }
        internal override int mainRendererIndex { get; set; } = 0;
        internal override float sortPosition { get; set; } = 1f;

        internal override void InitializeCharacter()
        {
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