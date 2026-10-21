using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Keystones
{
    /// <summary>
    /// Lives on every body that has a Keystone slot. Builds stacks from the body's own hits (server only)
    /// and decays them when the body stops hitting things.
    /// </summary>
    public class KeystoneHandler : MonoBehaviour, IOnDamageDealtServerReceiver
    {
        private const float DecayInterval = 0.5f;

        private CharacterBody body;
        private HealthComponent healthComponent;
        private GenericSkill keystoneSlot;
        private float uptimeStopwatch;
        private float decayStopwatch;

        public KeystoneType CurrentKeystone
        {
            get
            {
                if (!keystoneSlot) return KeystoneType.None;
                return KeystoneContent.TypeOf(keystoneSlot.skillDef);
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
            // Replaces SettMod's "settDamage" tag: any real hit counts, DoT ticks and other zero-proc damage don't.
            if (damageReport.damageInfo.procCoefficient <= 0f) return;

            KeystoneType keystone = CurrentKeystone;
            if (keystone == KeystoneType.None) return;

            uptimeStopwatch = 0f;

            switch (keystone)
            {
                case KeystoneType.Conqueror:
                    AddStack(KeystoneContent.conquerorBuff, KeystoneConfig.conquerorMaxStacks.Value);
                    if (body.GetBuffCount(KeystoneContent.conquerorBuff) >= KeystoneConfig.conquerorMaxStacks.Value && healthComponent)
                    {
                        healthComponent.Heal(damageReport.damageDealt * KeystoneConfig.conquerorHealFraction.Value, default(ProcChainMask), true);
                    }
                    break;

                case KeystoneType.LethalTempo:
                    AddStack(KeystoneContent.lethalBuff, KeystoneConfig.lethalMaxStacks.Value);
                    break;

                case KeystoneType.PhaseRush:
                    CharacterBody victim = damageReport.victimBody;
                    if (victim.GetBuffCount(KeystoneContent.phaseRushDebuff) < 3)
                    {
                        victim.AddTimedBuff(KeystoneContent.phaseRushDebuff, KeystoneConfig.phaseRushWindow.Value);
                    }
                    if (victim.GetBuffCount(KeystoneContent.phaseRushDebuff) >= 3)
                    {
                        GrantPhaseRush(body);
                    }
                    break;

                case KeystoneType.Electrocute:
                    ElectrocuteHandler electrocute = damageReport.victimBody.GetComponent<ElectrocuteHandler>();
                    if (!electrocute)
                    {
                        electrocute = damageReport.victimBody.gameObject.AddComponent<ElectrocuteHandler>();
                        electrocute.attackerBody = body;
                    }
                    electrocute.AddStack();
                    break;
            }
        }

        internal static void GrantPhaseRush(CharacterBody target)
        {
            if (target.GetBuffCount(KeystoneContent.phaseRushSpeedBuff) < 1)
            {
                target.AddTimedBuff(KeystoneContent.phaseRushSpeedBuff, KeystoneConfig.phaseRushSpeedDuration.Value);
            }
        }

        private void AddStack(BuffDef buff, int maxStacks)
        {
            if (body.GetBuffCount(buff) < maxStacks) body.AddBuff(buff);
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active || !body) return;

            BuffDef stackBuff;
            float duration;
            switch (CurrentKeystone)
            {
                case KeystoneType.Conqueror:
                    stackBuff = KeystoneContent.conquerorBuff;
                    duration = KeystoneConfig.conquerorDuration.Value;
                    break;
                case KeystoneType.LethalTempo:
                    stackBuff = KeystoneContent.lethalBuff;
                    duration = KeystoneConfig.lethalDuration.Value;
                    break;
                default:
                    return;
            }

            if (body.GetBuffCount(stackBuff) <= 0)
            {
                uptimeStopwatch = 0f;
                return;
            }

            uptimeStopwatch += Time.fixedDeltaTime;
            decayStopwatch += Time.fixedDeltaTime;

            // Out of combat for longer than the stack duration: drop one stack every DecayInterval.
            if (uptimeStopwatch > duration && decayStopwatch >= DecayInterval)
            {
                body.RemoveBuff(stackBuff);
                decayStopwatch = 0f;
            }
        }
    }
}
