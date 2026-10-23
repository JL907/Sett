using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Runes
{
    internal static class KeystoneContent
    {
        internal static readonly Keystone[] keystones =
        {
            new Conqueror(),
            new LethalTempo(),
            new StormraidersSurge(),
            new Electrocute(),
            new GraspOfTheUndying(),
        };

        internal static SkillFamily keystoneFamily;

        internal static readonly List<BuffDef> buffDefs = new List<BuffDef>();
        internal static readonly List<SkillDef> skillDefs = new List<SkillDef>();
        internal static readonly List<EffectDef> effectDefs = new List<EffectDef>();
        private static GameObject prefabHolder;
        private static readonly Dictionary<SkillDef, Keystone> keystoneBySkill = new Dictionary<SkillDef, Keystone>();

        internal static Keystone Get(SkillDef skillDef)
        {
            Keystone keystone;
            if (skillDef != null && keystoneBySkill.TryGetValue(skillDef, out keystone)) return keystone;
            return null;
        }

        internal static void Init()
        {
            Sprite noneIcon = LoadSprite("None_rune");

            List<SkillFamily.Variant> variants = new List<SkillFamily.Variant>();
            variants.Add(Variant(AddSkill("NONE", noneIcon, null)));
            LanguageAPI.Add("KEYSTONE_NONE_NAME", "<color=#9a9a9a>No Keystone</color>");
            LanguageAPI.Add("KEYSTONE_NONE_DESC", "Play without a keystone.");

            foreach (Keystone keystone in keystones)
            {
                keystone.icon = LoadSprite(keystone.IconName) ?? noneIcon;
                keystone.CreateBuffs();
                keystone.CreateEffects();
                variants.Add(Variant(AddSkill(keystone.Id, keystone.icon, keystone)));

                LanguageAPI.Add("KEYSTONE_" + keystone.Id + "_NAME", "<color=#ffa700>" + keystone.DisplayName + "</color>");
                LanguageAPI.Add("KEYSTONE_" + keystone.Id + "_DESC", keystone.Description());
            }

            keystoneFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (keystoneFamily as ScriptableObject).name = "KeystoneFamily";
            keystoneFamily.defaultVariantIndex = 0;
            keystoneFamily.variants = variants.ToArray();
        }

        internal static BuffDef AddBuff(string name, Sprite icon, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = name;
            buffDef.buffColor = Color.white;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = icon;
            buffDefs.Add(buffDef);
            return buffDef;
        }

        internal static GameObject CreatePrefab(string name)
        {
            if (!prefabHolder)
            {
                prefabHolder = new GameObject("RunesPrefabs");
                prefabHolder.SetActive(false);
                Object.DontDestroyOnLoad(prefabHolder);
            }
            GameObject prefab = new GameObject(name);
            prefab.transform.SetParent(prefabHolder.transform, false);
            return prefab;
        }

        internal static void AddEffect(GameObject prefab)
        {
            effectDefs.Add(new EffectDef(prefab) { shouldPool = false });
        }

        private static SkillDef AddSkill(string id, Sprite icon, Keystone keystone)
        {
            SkillDef skillDef = ScriptableObject.CreateInstance<SkillDef>();
            skillDef.skillName = "KEYSTONE_" + id;
            (skillDef as ScriptableObject).name = "KEYSTONE_" + id;
            skillDef.skillNameToken = "KEYSTONE_" + id + "_NAME";
            skillDef.skillDescriptionToken = "KEYSTONE_" + id + "_DESC";
            skillDef.icon = icon;
            skillDef.baseMaxStock = 1;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;
            skillDef.isCombatSkill = false;
            skillDef.fullRestockOnAssign = true;
            skillDef.keywordTokens = new string[0];

            skillDefs.Add(skillDef);
            if (keystone != null) keystoneBySkill[skillDef] = keystone;
            return skillDef;
        }

        private static SkillFamily.Variant Variant(SkillDef skillDef)
        {
            return new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }

        private static Sprite LoadSprite(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream("Runes.Icons." + name + ".png"))
            {
                if (stream == null)
                {
                    KeystonePlugin.Log.LogError("Missing embedded icon: " + name);
                    return null;
                }

                byte[] bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                ImageConversion.LoadImage(texture, bytes);
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.filterMode = FilterMode.Bilinear;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
    }

    internal class KeystoneContentPack : IContentPackProvider
    {
        private readonly ContentPack contentPack = new ContentPack();
        public string identifier => KeystonePlugin.GUID;

        internal void Initialize()
        {
            ContentManager.collectContentPackProviders += add => add(this);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            contentPack.buffDefs.Add(KeystoneContent.buffDefs.ToArray());
            contentPack.skillDefs.Add(KeystoneContent.skillDefs.ToArray());
            contentPack.effectDefs.Add(KeystoneContent.effectDefs.ToArray());
            contentPack.skillFamilies.Add(new[] { KeystoneContent.keystoneFamily });
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
