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
        private bool killQuotePlayed;
        private uint activePlayID;
        private GameObject gameObject;

        private void Awake()
        {
            this.gameObject = base.gameObject;
            On.RoR2.Stage.Start += (orig, self) =>
            {
                this.quotePlayed = false;
                this.killQuotePlayed = false;
                orig(self);
            };

            On.RoR2.TeleporterInteraction.OnInteractionBegin += (orig,self,activator) =>
            {
                this.quotePlayed = false;
                this.killQuotePlayed = false;
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
                        this.activeKillQuotePlayID = Util.PlaySound("SettBossKill", this.gameObject);
                        killQuotePlayed = true;
                    }
                }
                orig(self, damageReport);
            };
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

