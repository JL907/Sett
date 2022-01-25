using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates.Keystone
{
    public class ElectrocuteHandler : MonoBehaviour
    {
        public CharacterBody attackerBody;
        public float electrocuteCD = 5f;
        public float stopwatch = 0.0f;
        private static readonly int maxElectrocuteStacks = 3;
        private CharacterBody body;
        private int electrocuteStacks;
        private bool fired;
        private HurtBox hurtBox;

        public void AddStack()
        {
            if (GetStacks() < maxElectrocuteStacks && !fired)
            {
                this.body.AddTimedBuff(Modules.Buffs.electrocuteDebuff.buffIndex, 3);
                GetStacks();
            }
        }

        public int GetStacks()
        {
            this.electrocuteStacks = this.body.GetBuffCount(Modules.Buffs.electrocuteDebuff);
            return this.electrocuteStacks;
        }

        public void SetStacks(int num)
        {
            this.body.SetBuffCount(Modules.Buffs.electrocuteDebuff.buffIndex, num);
            GetStacks();
        }

        private void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.hurtBox = this.body.mainHurtBox;
            AddStack();
        }

        private void FireLightning()
        {
            if (GetStacks() >= maxElectrocuteStacks)
            {
                SetStacks(0);
                fired = true;

                float _level = Mathf.Floor(this.attackerBody.level / 4f);

                if (hurtBox)
                {
                    float damagecoefficient = (6f + (_level * 0.75f));
                    OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
                    {
                        attacker = attackerBody.gameObject,
                        damageColorIndex = DamageColorIndex.Item,
                        damageValue = this.attackerBody.damage * damagecoefficient,
                        isCrit = Util.CheckRoll(attackerBody.crit, attackerBody.master),
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 1f,
                        target = hurtBox
                    }); ; ;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!fired && NetworkServer.active)
            {
                FireLightning();
            }

            if (stopwatch < this.electrocuteCD)
            {
                stopwatch += Time.fixedDeltaTime;
            }

            if (stopwatch >= this.electrocuteCD)
            {
                Destroy(this);
            }
        }
    }
}