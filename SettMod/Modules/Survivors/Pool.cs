using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod.Modules.Survivors
{
    internal class Pool : Sett
    {
        internal static Material poolMat = Modules.Assets.CreateMaterial("poolSettMat");

        internal override BodyInfo bodyInfo { get; set; } = new BodyInfo
        {
            bodyName = "PoolBody",
            bodyNameToken = "POOL_SETT_NAME",
            bodyColor = new Color(.3781f, .1324f, .4894f),
            characterPortrait = Modules.Assets.mainAssetBundle.LoadAsset<Texture>("pool_square"),
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

        internal override string bodyName { get; set; } = "Pool";
        internal override GameObject bodyPrefab { get; set; }
        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override CustomRendererInfo[] customRendererInfos { get; set; } = new CustomRendererInfo[] {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = poolMat,
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
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region Chroma1

            SkinDef chroma1 = Modules.Skins.CreateSkinDef("default",
                Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
                defaultRenderers,
                mainRenderer,
                model);

            chroma1.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma1.icon = LoadoutAPI.CreateSkinIcon(
                new Color(195f / 255f, 2f / 255f, 86f / 255f), //purple
                new Color(245f / 255f, 197f / 255f, 4f / 255f), // orange
                new Color(230f / 255f, 230f / 255f, 230f / 255f), //white
                new Color(5f / 255f, 208f / 255f, 25f / 255f)); // green

            skins.Add(chroma1);

            #endregion Chroma1

            #region Chroma2

            Material chroma2Mat = Modules.Assets.CreateMaterial("skin11_0");
            CharacterModel.RendererInfo[] chroma2RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma2RendererInfos, 0);
            chroma2RendererInfos[0].defaultMaterial = chroma2Mat;
            SkinDef chroma2 = Modules.Skins.CreateSkinDef("Ruby",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma2RendererInfos,
            mainRenderer,
            model);

            chroma2.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma2.icon = LoadoutAPI.CreateSkinIcon(
                new Color(66f / 255f, 183f / 255f, 233f / 255f), //blue
                new Color(202f / 255f, 197f / 255f, 88f / 255f), //purple
                new Color(230f / 255f, 230f / 255f, 230f / 255f), //white
                new Color(5f / 255f, 208f / 255f, 25f / 255f)); // green

            skins.Add(chroma2);

            #endregion Chroma2

            #region Chroma3

            Material chroma3Mat = Modules.Assets.CreateMaterial("skin12_0");
            CharacterModel.RendererInfo[] chroma3RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma3RendererInfos, 0);
            chroma3RendererInfos[0].defaultMaterial = chroma3Mat;
            SkinDef chroma3 = Modules.Skins.CreateSkinDef("Catseye",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma3RendererInfos,
            mainRenderer,
            model);

            chroma3.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma3.icon = LoadoutAPI.CreateSkinIcon(
                new Color(131f / 255f, 70f / 255f, 0f / 255f), //brown
                new Color(255f / 255f, 255f / 255f, 0f / 255f), //yellow
                new Color(221f / 255f, 124f / 255f, 255f / 255f), //lav
                new Color(5f / 255f, 208f / 255f, 25f / 255f)); // green

            skins.Add(chroma3);

            #endregion Chroma3

            #region Chroma4

            Material chroma4Mat = Modules.Assets.CreateMaterial("skin13_0");
            CharacterModel.RendererInfo[] chroma4RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma4RendererInfos, 0);
            chroma4RendererInfos[0].defaultMaterial = chroma4Mat;
            SkinDef chroma4 = Modules.Skins.CreateSkinDef("Aquamarine",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma4RendererInfos,
            mainRenderer,
            model);

            chroma4.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma4.icon = LoadoutAPI.CreateSkinIcon(
                new Color(124f / 255f, 238f / 255f, 255f / 255f), //light blue
                new Color(18f / 255f, 165f / 255f, 250f / 255f), //light blue
                new Color(0f / 255f, 77f / 255f, 121f / 255f), //dark blue
                new Color(5f / 255f, 208f / 255f, 25f / 255f)); // green

            skins.Add(chroma4);

            #endregion Chroma4

            #region Chroma5

            Material chroma5Mat = Modules.Assets.CreateMaterial("skin14_0");
            CharacterModel.RendererInfo[] chroma5RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma5RendererInfos, 0);
            chroma5RendererInfos[0].defaultMaterial = chroma5Mat;
            SkinDef chroma5 = Modules.Skins.CreateSkinDef("Amethyst",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma5RendererInfos,
            mainRenderer,
            model);

            chroma5.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma5.icon = LoadoutAPI.CreateSkinIcon(
                new Color(5f / 255f, 208f / 255f, 25f / 255f), //green
                new Color(246f / 255f, 104f / 255f, 255f / 255f), //pink
                new Color(124f / 255f, 238f / 255f, 255f / 255f), //light blue
                new Color(177f / 255f, 43f / 255f, 226f / 255f)); // purple

            skins.Add(chroma5);

            #endregion Chroma5

            #region Chroma6

            Material chroma6Mat = Modules.Assets.CreateMaterial("skin15_0");
            CharacterModel.RendererInfo[] chroma6RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma6RendererInfos, 0);
            chroma6RendererInfos[0].defaultMaterial = chroma6Mat;
            SkinDef chroma6 = Modules.Skins.CreateSkinDef("Rose Quartz",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma6RendererInfos,
            mainRenderer,
            model);

            chroma6.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma6.icon = LoadoutAPI.CreateSkinIcon(
                new Color(124f / 255f, 238f / 255f, 255f / 255f), //light blue
                new Color(253f / 255f, 115f / 255f, 244f / 255f), //pink
                new Color(163f / 255f, 20f / 255f, 225f / 255f), //purple
                new Color(2f / 255f, 97f / 255f, 12f / 255f)); // dark green

            skins.Add(chroma6);

            #endregion Chroma6

            #region Chroma7

            Material chroma7Mat = Modules.Assets.CreateMaterial("skin16_0");
            CharacterModel.RendererInfo[] chroma7RendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(chroma7RendererInfos, 0);
            chroma7RendererInfos[0].defaultMaterial = chroma7Mat;
            SkinDef chroma7 = Modules.Skins.CreateSkinDef("Pearl",
            Assets.mainAssetBundle.LoadAsset<Sprite>("sett_square"),
            chroma7RendererInfos,
            mainRenderer,
            model);

            chroma7.meshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    mesh = mainRenderer.sharedMesh,
                    renderer = mainRenderer
                },
            };
            chroma7.icon = LoadoutAPI.CreateSkinIcon(
                new Color(163f / 255f, 20f / 255f, 225f / 255f), //purple
                new Color(156f / 255f, 255f / 255f, 251f / 255f), //light blue
                new Color(245f / 247f, 20f / 255f, 209f / 255f), //whiteyellow
                new Color(140f / 255f, 63f / 255f, 255f / 255f)); // dark green

            skins.Add(chroma7);

            #endregion Chroma7

            skinController.skins = skins.ToArray();
        }

        internal override void InitializeUnlockables()
        {
        }

        private static CharacterModel.RendererInfo[] SkinRendererInfos(CharacterModel.RendererInfo[] defaultRenderers, Material[] materials)
        {
            CharacterModel.RendererInfo[] newRendererInfos = new CharacterModel.RendererInfo[defaultRenderers.Length];
            defaultRenderers.CopyTo(newRendererInfos, 0);

            newRendererInfos[0].defaultMaterial = materials[0];
            newRendererInfos[1].defaultMaterial = materials[1];
            newRendererInfos[instance.mainRendererIndex].defaultMaterial = materials[2];

            return newRendererInfos;
        }
    }
}