using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace Keystones
{
    /// <summary>
    /// Added to an enemy (server side) the first time it's hit by someone running Electrocute.
    /// Three stacks fire a lightning strike, then the target is immune until the component expires.
    /// </summary>
    public class ElectrocuteHandler : MonoBehaviour
    {
        private const int MaxStacks = 3;

        public CharacterBody attackerBody;

        private CharacterBody body;
        private float stopwatch;
        private bool fired;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
        }

        public void AddStack()
        {
            if (fired || !body) return;
            if (body.GetBuffCount(KeystoneContent.electrocuteDebuff) < MaxStacks)
            {
                body.AddTimedBuff(KeystoneContent.electrocuteDebuff, KeystoneConfig.electrocuteWindow.Value);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!fired && body && body.GetBuffCount(KeystoneContent.electrocuteDebuff) >= MaxStacks)
            {
                Fire();
            }

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= KeystoneConfig.electrocuteCooldown.Value)
            {
                Destroy(this);
            }
        }

        private void Fire()
        {
            fired = true;
            body.SetBuffCount(KeystoneContent.electrocuteDebuff.buffIndex, 0);

            if (!attackerBody || !body.mainHurtBox) return;

            float levelSteps = Mathf.Floor(attackerBody.level / 4f);
            float coefficient = KeystoneConfig.electrocuteDamage.Value + levelSteps * KeystoneConfig.electrocuteDamagePerLevelStep.Value;

            OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
            {
                attacker = attackerBody.gameObject,
                damageColorIndex = DamageColorIndex.Item,
                damageValue = attackerBody.damage * coefficient,
                isCrit = Util.CheckRoll(attackerBody.crit, attackerBody.master),
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                target = body.mainHurtBox
            });
        }
    }
}
