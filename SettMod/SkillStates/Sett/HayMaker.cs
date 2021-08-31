using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using System.Collections.Generic;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        protected float startUp = 0.9f;
        protected float EarlyExitTime = 1.2f;
        protected float baseDuration = 3.55f;

        public static float hayMakerRadius = 55f;
        public static float hayMakerDamageCoefficient = 8f;
        public static float hayMakerProcCoefficient = 1f;
        public static float hayMakerForce = 1000f;

        public float duration;

        private bool hasFired;
        protected Animator animator;

        protected float stopwatch;


        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.hasFired = false;
            this.duration = this.baseDuration / base.attackSpeedStat;
            base.characterMotor.velocity = Vector3.zero;
            base.PlayAnimation("Fullbody, Override", "HayMaker", "HayMaker.playbackRate", this.duration);
            Util.PlaySound("SettWSFX", base.gameObject);

            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(Modules.Buffs.armorBuff, 1f);
                base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f);
            }
        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.OnExit();
        }


        protected virtual void OnHitEnemyAuthority()
        {

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if(this.stopwatch >= this.startUp && !this.hasFired && base.isAuthority)
            {
                this.hasFired = true;
                Util.PlaySound("SettWVO", base.gameObject);

                BlastAttack blastAttack = new BlastAttack();
                blastAttack.radius = HayMaker.hayMakerRadius;
                blastAttack.procCoefficient = HayMaker.hayMakerProcCoefficient;
                blastAttack.position = base.characterBody.corePosition;
                blastAttack.attacker = base.gameObject;
                blastAttack.crit = base.RollCrit();
                blastAttack.baseDamage = base.characterBody.damage * HayMaker.hayMakerDamageCoefficient;
                blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                blastAttack.baseForce = HayMaker.hayMakerForce;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                blastAttack.damageType = DamageType.Stun1s;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
                blastAttack.Fire();


                AkSoundEngine.SetRTPCValue("M2_Charge", 100f);
                Util.PlaySound("SettRImpact", base.gameObject);

                for (int i = 0; i <= 4; i += 1)
                {
                    Vector3 effectPosition = base.characterBody.corePosition + (UnityEngine.Random.insideUnitSphere * 4f);
                    effectPosition.y = base.characterBody.corePosition.y;
                    EffectManager.SpawnEffect(EntityStates.LemurianBruiserMonster.SpawnState.spawnEffectPrefab, new EffectData
                    {
                        origin = effectPosition,
                        scale = 2f
                    }, true);
                }
            }


            if (this.stopwatch >= this.EarlyExitTime && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}