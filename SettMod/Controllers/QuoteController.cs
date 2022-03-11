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
        private bool killQuotePlayed;
        private bool teleportQuotePlayed;
        private GameObject gameObject;

        private void Awake()
        {
            this.gameObject = base.gameObject;
            On.RoR2.Stage.Start += (orig, self) =>
            {
                this.ResetPlayed();
                orig(self);
            };

            On.RoR2.TeleporterInteraction.OnInteractionBegin += (orig,self,activator) =>
            {
                TeleporterInteraction.ActivationState activationState = self.activationState;
                switch (activationState)
                {

                    default:
                    case TeleporterInteraction.ActivationState.Idle:
                        this.ResetPlayed();
                        break;
                    case TeleporterInteraction.ActivationState.Charged:
                        if (!teleportQuotePlayed)
                        {
                            this.teleportQuotePlayed = true;
                            this.activeTeleportQuotePlayID = Util.PlaySound("SettTeleport", this.gameObject);
                        }
                        break;
                }
                orig(self,activator);
            };

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, damageReport) =>
            {
                if (damageReport is null) return;
                if (damageReport.victimBody is null) return;
                if (damageReport.attackerBody is null) return;
                if (damageReport.victimBody.isBoss || damageReport.victimBody.isChampion)
                {
                    if (!killQuotePlayed)
                    {
                        killQuotePlayed = true;
                        this.activeKillQuotePlayID = Util.PlaySound("SettBossKill", this.gameObject);
                    }
                }
                orig(self, damageReport);
            };
        }

        private void ResetPlayed()
        {
            this.quotePlayed = false;
            this.killQuotePlayed = false;
            this.teleportQuotePlayed = false;
        }

        private void FixedUpdate()
        {
            if (!quotePlayed) SearchForTargets();
        }

        private void PlayQuote()
        {
            this.activePlayID = Util.PlaySound("SettBossQuote", base.gameObject);
        }

        private void OnExit()
        {
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);
            if (this.activeKillQuotePlayID != 0) AkSoundEngine.StopPlayingID(this.activeKillQuotePlayID);
            if (this.activeTeleportQuotePlayID != 0) AkSoundEngine.StopPlayingID(this.activeTeleportQuotePlayID);
        }

        protected void SearchForTargets()
        {
            foreach (Collider collider in Physics.OverlapSphere(base.transform.position, 100f))
            {
                HealthComponent component = collider.GetComponent<HealthComponent>();
                if (component)
                {
                    TeamComponent component2 = collider.GetComponent<TeamComponent>();
                    bool flag = false;
                    if (component2)
                    {
                        flag = (component2.teamIndex == TeamComponent.GetObjectTeam(base.gameObject));
                    }
                    if (!flag)
                    {
                        if (component.body.isChampion || component.body.isBoss)
                        {
                            this.quotePlayed = true;
                            PlayQuote();
                        }
                    }
                }
            }
        }
    }
}

