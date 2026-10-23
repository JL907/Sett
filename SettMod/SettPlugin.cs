using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using SettMod.Modules;
using SettMod.Modules.Survivors;
using SettMod.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.sound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.networking", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.loadout", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.damagetype", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(Runes.KeystonePlugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class SettPlugin : BaseUnityPlugin
    {
        public const string developerPrefix = "SETT";

        public const string MODNAME = "Sett";

        public const string MODUID = "com.Lemonlust.Sett";

        public const string MODVERSION = "4.8.1";
        public static SettPlugin instance;
        public static DamageAPI.ModdedDamageType settDamage;
        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();

        private GritGauge gritGauge;

        public void OnDestroy()
        {
            try
            {
                UnHooks();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        private void Awake()
        {
            instance = this;
            try
            {
                settDamage = DamageAPI.ReserveDamageType();
                Modules.Assets.Initialize();
                Modules.Config.ReadConfig();
                Modules.CameraParams.InitializeParams();
                Modules.States.RegisterStates();
                NetworkingAPI.RegisterMessageType<SkillStates.SyncSettGrab>();
                NetworkingAPI.RegisterMessageType<SkillStates.SyncSettRelease>();
                NetworkingAPI.RegisterMessageType<SyncAddGrit>();
                Modules.Buffs.RegisterBuffs();
                Modules.Projectiles.RegisterProjectiles();
                Modules.Tokens.AddTokens();
                Modules.ItemDisplays.PopulateDisplays();

                new Sett().Initialize();

                new Modules.ContentPacks().Initialize();

                RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;

                Hook();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                float _level = Mathf.Floor(self.level / 4f);
                if (self.HasBuff(Modules.Buffs.regenBuff))
                {
                    float count = self.GetBuffCount(Modules.Buffs.regenBuff);
                    self.regen += count * (0.25f + (_level * 0.25f));
                }
            }
        }

        private void CreateGritGauge(RoR2.UI.HUD hud)
        {
            if (gritGauge || hud == null) return;

            gritGauge = hud.GetComponentInChildren<GritGauge>(true);
            if (!gritGauge) gritGauge = GritGauge.Create(hud);
        }

        private void Hook()
        {
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer += PickupPickerController_FixedUpdateServer;
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            CreateGritGauge(self);
            orig(self);
        }

        private void HUD_onHudTargetChangedGlobal(RoR2.UI.HUD obj)
        {
            if (obj && obj.targetBodyObject && gritGauge)
            {
                var grit = obj.targetBodyObject.GetComponent<GritComponent>();
                if (grit)
                {
                    gritGauge.gameObject.SetActive(true);
                    gritGauge.source = grit;
                }
                else
                {
                    gritGauge.gameObject.SetActive(false);
                    gritGauge.source = null;
                }
            }
        }

        private void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
        }

        private void PickupPickerController_FixedUpdateServer(On.RoR2.PickupPickerController.orig_FixedUpdateServer orig, PickupPickerController self)
        {
            CharacterMaster currentParticipantMaster = self.networkUIPromptController.currentParticipantMaster;
            if (currentParticipantMaster)
            {
                CharacterBody body = currentParticipantMaster.GetBody();
                var interactor = (body) ? body.GetComponent<Interactor>() : null;
                if (!body || (body.inputBank.aimOrigin - self.transform.position).sqrMagnitude > ((interactor) ? Math.Pow((interactor.maxInteractionDistance + self.cutoffDistance), 2f) : (self.cutoffDistance * self.cutoffDistance)))
                {
                    self.networkUIPromptController.SetParticipantMaster(null);
                }
            }
        }

        private void UnHooks()
        {
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.UI.HUD.Awake -= HUD_Awake;
            RoR2.UI.HUD.onHudTargetChangedGlobal -= HUD_onHudTargetChangedGlobal;
            On.RoR2.PickupPickerController.FixedUpdateServer -= PickupPickerController_FixedUpdateServer;
        }
    }
}