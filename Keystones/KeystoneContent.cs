using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Keystones
{
    public enum KeystoneType
    {
        None = 0,
        Conqueror = 1,
        LethalTempo = 2,
        PhaseRush = 4,
        Electrocute = 8
    }

    /// <summary>Buffs, skill defs, the shared Keystone skill family, tokens and icons.</summary>
    internal static class KeystoneContent
    {
        internal static BuffDef conquerorBuff;
        internal static BuffDef lethalBuff;
        internal static BuffDef phaseRushDebuff;
        internal static BuffDef phaseRushSpeedBuff;
        internal static BuffDef electrocuteDebuff;

        internal static SkillFamily keystoneFamily;

        internal static readonly List<BuffDef> buffDefs = new List<BuffDef>();
        internal static readonly List<SkillDef> skillDefs = new List<SkillDef>();
        private static readonly Dictionary<SkillDef, KeystoneType> typeBySkill = new Dictionary<SkillDef, KeystoneType>();

        internal static KeystoneType TypeOf(SkillDef skillDef)
        {
            KeystoneType type;
            if (skillDef != null && typeBySkill.TryGetValue(skillDef, out type)) return type;
            return KeystoneType.None;
        }

        internal static void Init()
        {
            Sprite conquerorIcon = LoadSprite("Conqueror_rune");
            Sprite lethalIcon = LoadSprite("Lethal_Tempo_rune");
            Sprite phaseIcon = LoadSprite("Phase_Rush_rune");
            Sprite electrocuteIcon = LoadSprite("Electrocute_rune");
            Sprite noneIcon = LoadSprite("None_rune");

            // Buff names are prefixed so they don't collide with the old SettMod buffs if someone runs an outdated Sett.
            conquerorBuff = AddBuff("KeystoneConquerorBuff", conquerorIcon, true, false);
            lethalBuff = AddBuff("KeystoneLethalTempoBuff", lethalIcon, true, false);
            phaseRushDebuff = AddBuff("KeystonePhaseRushDebuff", phaseIcon, true, true);
            phaseRushSpeedBuff = AddBuff("KeystonePhaseRushSpeedBuff", phaseIcon, false, false);
            electrocuteDebuff = AddBuff("KeystoneElectrocuteDebuff", electrocuteIcon, true, true);

            keystoneFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (keystoneFamily as ScriptableObject).name = "KeystoneFamily";
            keystoneFamily.defaultVariantIndex = 0;
            keystoneFamily.variants = new[]
            {
                // "None" first so vanilla survivors are unchanged until a player opts in.
                Variant(AddSkill("NONE", noneIcon, KeystoneType.None)),
                Variant(AddSkill("CONQUEROR", conquerorIcon, KeystoneType.Conqueror)),
                Variant(AddSkill("LETHAL_TEMPO", lethalIcon, KeystoneType.LethalTempo)),
                Variant(AddSkill("PHASE_RUSH", phaseIcon, KeystoneType.PhaseRush)),
                Variant(AddSkill("ELECTROCUTE", electrocuteIcon, KeystoneType.Electrocute)),
            };

            AddTokens();
        }

        private static BuffDef AddBuff(string name, Sprite icon, bool canStack, bool isDebuff)
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

        private static SkillDef AddSkill(string id, Sprite icon, KeystoneType type)
        {
            // Passive-style skill: it is never activated, the slot only records the player's choice.
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
            typeBySkill[skillDef] = type;
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

        private static void AddTokens()
        {
            const string hit = "<color=#c9aa71>Hits with a proc coefficient</color>";

            LanguageAPI.Add("KEYSTONE_NONE_NAME", "<color=#9a9a9a>No Keystone</color>");
            LanguageAPI.Add("KEYSTONE_NONE_DESC", "Play without a keystone.");

            LanguageAPI.Add("KEYSTONE_CONQUEROR_NAME", "<color=#ffa700>Conqueror</color>");
            LanguageAPI.Add("KEYSTONE_CONQUEROR_DESC",
                hit + " against enemies grant <color=#ffffff>1</color> stack of Conqueror, up to " + KeystoneConfig.conquerorMaxStacks.Value + " stacks. " +
                "Each stack grants <color=#f68835>" + Num(KeystoneConfig.conquerorDamagePerStack.Value) + "</color> " +
                "<color=#d62d20>(+" + Num(KeystoneConfig.conquerorDamagePerStackPerLevelStep.Value) + " every 4 levels)</color> bonus base damage. " +
                "While fully stacked, <color=#c9aa71>heal</color> for <color=#008744>" + Pct(KeystoneConfig.conquerorHealFraction.Value) + " of damage dealt</color>.");

            LanguageAPI.Add("KEYSTONE_LETHAL_TEMPO_NAME", "<color=#ffa700>Lethal Tempo</color>");
            LanguageAPI.Add("KEYSTONE_LETHAL_TEMPO_DESC",
                hit + " against enemies grant <color=#ffffff>1</color> stack of Lethal Tempo, up to " + KeystoneConfig.lethalMaxStacks.Value + " stacks. " +
                "Gain <color=#f68835>" + Pct(KeystoneConfig.lethalAttackSpeedPerStack.Value) + "</color> bonus attack speed per stack, up to " +
                "<color=#f68835>" + Pct(KeystoneConfig.lethalAttackSpeedPerStack.Value * KeystoneConfig.lethalMaxStacks.Value) + "</color> at maximum stacks.");

            LanguageAPI.Add("KEYSTONE_PHASE_RUSH_NAME", "<color=#ffa700>Phase Rush</color>");
            LanguageAPI.Add("KEYSTONE_PHASE_RUSH_DESC",
                hit + " apply <color=#c9aa71>1</color> stack to enemies. Applying <color=#ffffff>3</color> stacks to a target within " +
                Num(KeystoneConfig.phaseRushWindow.Value) + " seconds grants <color=#f68835>" + Pct(KeystoneConfig.phaseRushSpeed.Value) + "</color> " +
                "<color=#d62d20>(+" + Pct(KeystoneConfig.phaseRushSpeedPerLevelStep.Value) + " every 4 levels)</color> bonus <color=#c9aa71>movement speed</color> for " +
                Num(KeystoneConfig.phaseRushSpeedDuration.Value) + " seconds. Kills also grant the bonus.");

            LanguageAPI.Add("KEYSTONE_ELECTROCUTE_NAME", "<color=#ffa700>Electrocute</color>");
            LanguageAPI.Add("KEYSTONE_ELECTROCUTE_DESC",
                hit + " apply <color=#c9aa71>1</color> stack to enemies. Applying <color=#ffffff>3</color> stacks to a target within " +
                Num(KeystoneConfig.electrocuteWindow.Value) + " seconds strikes it with lightning for <color=#f68835>" + Pct(KeystoneConfig.electrocuteDamage.Value) + "</color> " +
                "<color=#d62d20>(+" + Pct(KeystoneConfig.electrocuteDamagePerLevelStep.Value) + " every 4 levels)</color> damage. " +
                Num(KeystoneConfig.electrocuteCooldown.Value) + " second cooldown per target.");
        }

        private static string Num(float v) { return v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture); }
        private static string Pct(float v) { return Num(v * 100f) + "%"; }

        private static Sprite LoadSprite(string name)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream("Keystones.Icons." + name + ".png"))
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
