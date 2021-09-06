using EntityStates;
using RoR2;
using RoR2.Projectile;
using SettMod.Modules;
using UnityEngine;

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
#pragma warning disable CS0169 // The field 'HayMaker.attack' is never used
        private OverlapAttack attack;
#pragma warning restore CS0169 // The field 'HayMaker.attack' is never used


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

                ProjectileManager.instance.FireProjectile(Modules.Projectiles.conePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject,
                        ((this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * HayMaker.hayMakerGritBonus)),
                        100f,
                        base.RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        -1f);
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