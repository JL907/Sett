using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Runes
{
    /// <summary>
    /// Lives on every body that has a Keystone slot. Passes the body's own hits (server only) to the
    /// selected keystone and gives keystones shared helpers for stacking buffs.
    /// </summary>
    public class KeystoneHandler : MonoBehaviour, IOnDamageDealtServerReceiver
    {
        private const float DecayInterval = 0.5f;

        internal CharacterBody body;
        internal HealthComponent healthComponent;
        private GenericSkill keystoneSlot;
        private float uptimeStopwatch;
        private float decayStopwatch;

        /// <summary>The selected keystone, or null for No Keystone.</summary>
        internal Keystone Current
        {
            get { return keystoneSlot ? KeystoneContent.Get(keystoneSlot.skillDef) : null; }
        }

        public KeystoneType CurrentKeystone
        {
            get
            {
                Keystone keystone = Current;
                return keystone != null ? keystone.Type : KeystoneType.None;
            }
        }

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            healthComponent = GetComponent<HealthComponent>();

            foreach (GenericSkill skill in GetComponents<GenericSkill>())
            {
                if (skill.skillFamily == KeystoneContent.keystoneFamily)
                {
                    keystoneSlot = skill;
                    break;
                }
            }
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            if (damageReport == null || damageReport.damageInfo == null) return;
            if (!damageReport.victimBody || damageReport.victimBody == body) return;
            if (damageReport.attackerBody != body) return;
            if (damageReport.victimTeamIndex == damageReport.attackerTeamIndex) return;
            Keystone keystone = Current;
            if (keystone == null) return;

            // Replaces SettMod's "settDamage" tag: any real hit counts, DoT ticks and other zero-proc damage don't,
            // unless the keystone asks for all damage.
            bool realHit = damageReport.damageInfo.procCoefficient > 0f;
            if (!realHit && keystone.RequiresProcCoefficient) return;

            if (realHit) uptimeStopwatch = 0f;
            keystone.OnHitServer(this, damageReport);
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || !body) return;

            Keystone keystone = Current;
            if (keystone != null) keystone.FixedUpdateServer(this);
        }

        /// <summary>Adds one stack of a self buff, up to the cap.</summary>
        internal void AddStack(BuffDef buff, int maxStacks)
        {
            if (body.GetBuffCount(buff) < maxStacks) body.AddBuff(buff);
        }

        /// <summary>
        /// Call from FixedUpdateServer. Once the body has gone longer than duration without a hit,
        /// drops one stack every DecayInterval.
        /// </summary>
        internal void DecayStacks(BuffDef buff, float duration)
        {
            if (body.GetBuffCount(buff) <= 0)
            {
                uptimeStopwatch = 0f;
                return;
            }

            uptimeStopwatch += Time.fixedDeltaTime;
            decayStopwatch += Time.fixedDeltaTime;

            if (uptimeStopwatch > duration && decayStopwatch >= DecayInterval)
            {
                body.RemoveBuff(buff);
                decayStopwatch = 0f;
            }
        }
    }
}
