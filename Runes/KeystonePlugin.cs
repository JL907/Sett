using BepInEx;
using BepInEx.Logging;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Runes
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class KeystonePlugin : BaseUnityPlugin
    {
        public const string GUID = "com.Lemonlust.Runes";
        public const string NAME = "Runes";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;
        internal static bool slotsInjected;

        private void Awake()
        {
            Log = Logger;
            try
            {
                KeystoneConfig.Init(Config);
                KeystoneContent.Init();
                new KeystoneContentPack().Initialize();

                // Fires after every mod's content pack is loaded but before BodyCatalog/SkillCatalog
                // build their caches, so slots added here show up in the loadout screen.
                ContentManager.onContentPacksAssigned += AddKeystoneSlots;

                On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
                GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;

                foreach (Keystone keystone in KeystoneContent.keystones)
                {
                    keystone.Hook();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        private void OnDestroy()
        {
            ContentManager.onContentPacksAssigned -= AddKeystoneSlots;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobal;

            foreach (Keystone keystone in KeystoneContent.keystones)
            {
                keystone.Unhook();
            }
        }

        private static void AddKeystoneSlots(HG.ReadOnlyArray<ReadOnlyContentPack> packs)
        {
            if (slotsInjected) return;
            slotsInjected = true;

            int added = 0;
            for (int i = 0; i < packs.Length; i++)
            {
                foreach (SurvivorDef survivorDef in packs[i].survivorDefs)
                {
                    if (!survivorDef || !survivorDef.bodyPrefab) continue;
                    try
                    {
                        if (ShouldHaveSlot(survivorDef))
                        {
                            AddSlot(survivorDef.bodyPrefab);
                            added++;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.LogError("Couldn't add a Keystone slot to " + survivorDef.bodyPrefab.name + ": " + e);
                    }
                }
            }
            Log.LogInfo("Added Keystone slot to " + added + " survivors.");
        }

        private static bool ShouldHaveSlot(SurvivorDef survivorDef)
        {
            string bodyName = survivorDef.bodyPrefab.name;
            if (KeystoneAPI.excluded.Contains(bodyName)) return false;

            // Always create the entry so every survivor shows up in the config file.
            bool enabledInConfig = KeystoneConfig.BindSurvivor(bodyName, survivorDef.displayNameToken ?? bodyName).Value;
            if (!enabledInConfig) return false;

            return KeystoneConfig.allSurvivors.Value || KeystoneAPI.registered.Contains(bodyName);
        }

        private static void AddSlot(GameObject bodyPrefab)
        {
            foreach (GenericSkill existing in bodyPrefab.GetComponents<GenericSkill>())
            {
                if (existing.skillFamily == KeystoneContent.keystoneFamily) return;
            }

            GenericSkill slot = bodyPrefab.AddComponent<GenericSkill>();
            slot.skillName = "Keystone";
            slot._skillFamily = KeystoneContent.keystoneFamily;
            slot.hideInCharacterSelect = false;

            if (!bodyPrefab.GetComponent<KeystoneHandler>())
            {
                bodyPrefab.AddComponent<KeystoneHandler>();
            }
        }

        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (!self) return;

            float levelSteps = Mathf.Floor(self.level / 4f);
            foreach (Keystone keystone in KeystoneContent.keystones)
            {
                keystone.RecalculateStats(self, levelSteps);
            }
        }

        private static void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active || damageReport == null) return;
            if (!damageReport.attackerBody || !damageReport.victimBody) return;
            if (damageReport.victimTeamIndex == damageReport.attackerTeamIndex) return;

            KeystoneHandler handler = damageReport.attackerBody.GetComponent<KeystoneHandler>();
            Keystone keystone = handler ? handler.Current : null;
            if (keystone != null)
            {
                keystone.OnKillServer(handler, damageReport);
            }
        }
    }
}
