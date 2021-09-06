using EntityStates;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using SettMod.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        protected float startUp = 0.8f;
        protected float EarlyExitTime = 1.2f;
        protected float baseDuration = 3.55f;
        public static float hayMakerRadius = 55f;
        public static float hayMakerDamageCoefficient = 16f;
        public static float hayMakerProcCoefficient = 1f;
        public static float hayMakerGritBonus = 0.5f;
        public static float hayMakerForce = 1000f;

        private float gritSnapShot;

        private Vector3 punchVector

        {
            get
            {
                //return base.characterDirection.forward.normalized;
                return base.inputBank.aimDirection;
            }
        }
        private OverlapAttack attack;


        public float duration;

        private bool hasFired;
        protected Animator animator;

        protected float stopwatch;


        public override void OnEnter()
        {
            base.OnEnter();
            if (base.isAuthority)
            {
                this.animator = base.GetModelAnimator();
                this.hasFired = false;
                this.duration = this.baseDuration / base.attackSpeedStat;
                base.characterMotor.velocity = Vector3.zero;
                base.PlayAnimation("Fullbody, Override", "HayMaker", "HayMaker.playbackRate", this.duration);
                Util.PlaySound("SettWSFX", base.gameObject);
                GritComponent gritComponent = base.GetComponent<GritComponent>();
                float currentGrit = gritComponent.GetCurrentGrit();
                this.gritSnapShot = currentGrit;
                base.healthComponent.AddBarrierAuthority(currentGrit);
                base.GetComponent<GritComponent>().AddGritAuthority(-currentGrit);
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

        private void Fire()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(this.punchVector);
                fireProjectileInfo.crit = base.RollCrit();
                fireProjectileInfo.damage = ((this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * HayMaker.hayMakerGritBonus));
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.projectilePrefab = Modules.Projectiles.conePrefab;
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.startUp && !this.hasFired && base.isAuthority)
            {
                this.hasFired = true;
                Util.PlaySound("SettWVO", base.gameObject);

                this.Fire();
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