using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;
using UnityEngine.Networking;


namespace SettMod.SkillStates
{
    public class Facebreakerold : BaseSkillState
    {
        protected DamageType damageType = DamageType.Stun1s;
        protected float damageCoefficient = 8f;
        protected float procCoefficient = 1f;
        protected float baseDuration = 0.5f;
        protected bool cancelled = false;
        protected float startUp = 0.22f;
        protected float attackEndTime = 0.4f;
        protected float hitStopDuration = 0.012f;
        protected float hitHopVelocity = 4f;
        protected float attackRecoil = 0.75f;

        protected float pushForce = 1500f;
        protected Vector3 bonusForce = Vector3.forward;

        protected string hitboxName = "FaceBreaker";

        protected string swingSoundString = "";
        protected string hitSoundString = "Hit";
        protected string muzzleString = "FaceBreakerCenter";
        protected GameObject swingEffectPrefab;
        protected GameObject hitEffectPrefab;
        protected NetworkSoundEventIndex impactSound;

        public float duration;
        private bool hasFired;
        private float hitPauseTimer;
        private OverlapAttack attack;
        protected bool inHitPause;
        private bool hasHopped;
        protected float stopwatch;
        protected Animator animator;
        private BaseState.HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;



        public override void OnEnter()
        {
            base.OnEnter();

            this.swingEffectPrefab = Modules.Assets.faceBreakerEffect;
            this.hitEffectPrefab = Modules.Assets.swordHitImpactEffect;
            this.animator = base.GetModelAnimator();
            this.hasFired = false;
            this.duration = this.baseDuration / this.attackSpeedStat;

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == this.hitboxName);
            }

            this.attack = new OverlapAttack();
            this.attack.damageType = this.damageType;
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = base.GetTeam();
            this.attack.damage = this.damageCoefficient * this.damageStat;
            this.attack.procCoefficient = this.procCoefficient;
            this.attack.hitEffectPrefab = this.hitEffectPrefab;
            this.attack.forceVector = this.bonusForce;
            this.attack.pushAwayForce = this.pushForce;
            this.attack.hitBoxGroup = hitBoxGroup;
            this.attack.isCrit = base.RollCrit();
            this.attack.impactSound = this.impactSound;

            base.PlayCrossfade("Fullbody, Override", "Facebreaker_Start", "FaceBreaker.playbackRate", this.startUp, 0.03f);
            Util.PlaySound("SettESFX", base.gameObject);

        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.OnExit();
        }


        protected virtual void PlaySwingEffect()
        {
            EffectManager.SimpleMuzzleFlash(this.swingEffectPrefab, base.gameObject, this.muzzleString, true);
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(this.hitSoundString, base.gameObject);

            

            if (!this.hasHopped)
            {
                if (base.characterMotor && !base.characterMotor.isGrounded && this.hitHopVelocity > 0f)
                {
                    base.SmallHop(base.characterMotor, this.hitHopVelocity);
                }

                this.hasHopped = true;
            }

            if (!this.inHitPause && this.hitStopDuration > 0f)
            {
                this.storedVelocity = base.characterMotor.velocity;
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "FaceBreaker_Miss.playbackRate");
                this.hitPauseTimer = this.hitStopDuration / this.attackSpeedStat;
                this.inHitPause = true;
            }
        }

        private void FireAttack()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                if (base.isAuthority)
                {
                    //this.PlaySwingEffect();
                    base.AddRecoil(-1f * this.attackRecoil, -2f * this.attackRecoil, -0.5f * this.attackRecoil, 0.5f * this.attackRecoil);
                    base.PlayCrossfade("Fullbody, Override", "Facebreaker_Miss", "FaceBreakerMiss.playbackRate", (this.duration * this.startUp), 0.1f);
                    Util.PlaySound("SettEVO", base.gameObject);
                }

                if (base.isAuthority)
                {
                    if (this.attack.Fire())
                    {

                        this.OnHitEnemyAuthority();
                    }
                }
            }
        }

        public override void FixedUpdate()
        {

            base.FixedUpdate();

            this.hitPauseTimer -= Time.fixedDeltaTime;

            if (this.hitPauseTimer <= 0f && this.inHitPause)
            {
                base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                this.inHitPause = false;
                base.characterMotor.velocity = this.storedVelocity;
            }

            if (!this.inHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("FaceBreakerMiss.playbackRate.playbackRate", 0f);
            }

            if (this.stopwatch >= (this.duration * this.startUp) && this.stopwatch <= (this.duration * this.attackEndTime))
            {
                this.FireAttack(); //fire after fixedage is more than the startup frames
            }

            if (this.stopwatch >= this.duration && base.isAuthority)
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