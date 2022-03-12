using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SettMod.Controllers
{
    public class QuoteController : MonoBehaviour
    {
        private bool quotePlayed;
        private uint activeKillQuotePlayID;
        private uint activePlayID;
        private uint activeTeleportQuotePlayID;
        private uint activeWalkQuotePlayID;
        private bool killQuotePlayed;
        private bool teleportQuotePlayed;
        private GameObject gameObject;

        private void Awake()
        {
            this.gameObject = base.gameObject;
            TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            BossGroup.onBossGroupStartServer += BossGroup_onBossGroupStartServer;
            BossGroup.onBossGroupDefeatedServer += BossGroup_onBossGroupDefeatedServer;
            Run.onRunAmbientLevelUp += Run_onRunAmbientLevelUp;
        }

        private void Run_onRunAmbientLevelUp(Run obj)
        {
            this.activeWalkQuotePlayID = Util.PlaySound("SettWalkQuote", this.gameObject);
        }

        private void BossGroup_onBossGroupDefeatedServer(BossGroup obj)
        {
            if (!killQuotePlayed)
            {
                killQuotePlayed = true;
                this.activeKillQuotePlayID = Util.PlaySound("SettBossKill", this.gameObject);
            }
        }

        private void BossGroup_onBossGroupStartServer(BossGroup obj)
        {
            if(!quotePlayed)
            {
                this.quotePlayed = true;
                this.activePlayID = Util.PlaySound("SettBossQuote", base.gameObject);
            }
        }

        private void TeleporterInteraction_onTeleporterFinishGlobal(TeleporterInteraction obj)
        {
            if (!teleportQuotePlayed)
            {
                this.teleportQuotePlayed = true;
                this.activeTeleportQuotePlayID = Util.PlaySound("SettTeleport", this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);
            if (this.activeKillQuotePlayID != 0) AkSoundEngine.StopPlayingID(this.activeKillQuotePlayID);
            if (this.activeTeleportQuotePlayID != 0) AkSoundEngine.StopPlayingID(this.activeTeleportQuotePlayID);
            if (this.activeWalkQuotePlayID != 0) AkSoundEngine.StopPlayingID(this.activeWalkQuotePlayID);
            TeleporterInteraction.onTeleporterFinishGlobal -= TeleporterInteraction_onTeleporterFinishGlobal;
            BossGroup.onBossGroupStartServer -= BossGroup_onBossGroupStartServer;
            BossGroup.onBossGroupDefeatedServer -= BossGroup_onBossGroupDefeatedServer;
            Run.onRunAmbientLevelUp -= Run_onRunAmbientLevelUp;
        }
    }
}

