using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates.Keystone
{
    public class ElectrocuteHandler : MonoBehaviour
    {
        private CharacterBody body;
        private HurtBox hurtBox;
        public float stopwatch = 0.0f;
        public float electrocuteCD = 10f;
        private int electrocuteStacks;
        private static readonly int maxElectrocuteStacks = 3;
        private bool fired;

        public CharacterBody attackerBody;


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

                float _level = Mathf.Floor(this.attackerBody.level  / 4f);

                if (hurtBox)
                {
                    float damagecoefficient = (3f + (_level * 0.75f));
                    OrbManager.instance.AddOrb(new LightningStrikeOrb
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
        public void AddStack()
        {
            if (GetStacks() < maxElectrocuteStacks && !fired)
            {
                this.body.AddTimedBuff(Modules.Buffs.electrocuteDebuff.buffIndex, 3);
                GetStacks();
            }
        }

        public void SetStacks(int num)
        {
            this.body.SetBuffCount(Modules.Buffs.electrocuteDebuff.buffIndex, num);
            GetStacks();
        }

        public int GetStacks()
        {
            this.electrocuteStacks = this.body.GetBuffCount(Modules.Buffs.electrocuteDebuff);
            return this.electrocuteStacks;
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
