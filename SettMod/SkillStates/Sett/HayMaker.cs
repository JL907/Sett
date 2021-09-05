using EntityStates;
using RoR2;
using RoR2.Projectile;
using SettMod.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
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
                base.StartAimMode(0.5f + this.duration, false);
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

        private void FireSecondaryRaysServer()
        {
            Ray aimRay = base.GetAimRay();
            TeamIndex team = base.GetTeam();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(team);
            bullseyeSearch.maxAngleFilter = 45f;
            bullseyeSearch.maxDistanceFilter = 35f;
            bullseyeSearch.searchOrigin = base.GetAimRay().origin;
            bullseyeSearch.searchDirection = this.punchVector;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.RefreshCandidates();



            List<HurtBox> list = bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(Util.IsValid)).ToList<HurtBox>();
            Transform transform = base.FindModelChild("SwingCenter");

            if (transform)
            {
                /*for (int i = 0; i < Mathf.Min(list.Count, 100f); i++)
                {
                    HurtBox hurtBox = list[i];
                    if (hurtBox)
                    {
                        LightningOrb lightningOrb = new LightningOrb();
                        lightningOrb.bouncedObjects = new List<HealthComponent>();
                        lightningOrb.attacker = base.gameObject;
                        lightningOrb.teamIndex = team;
                        lightningOrb.damageValue = (this.damageStat * HayMaker.hayMakerDamageCoefficient) + (HayMaker.hayMakerGritBonus * this.gritSnapShot);
                        lightningOrb.isCrit = base.RollCrit();
                        lightningOrb.origin = hurtBox.healthComponent.body.transform.position;
                        lightningOrb.bouncesRemaining = 0;
                        lightningOrb.lightningType = LightningOrb.LightningType.Loader;
                        lightningOrb.procCoefficient = 1f;
                        lightningOrb.target = hurtBox;
                        lightningOrb.canBounceOnSameTarget = false;
                        OrbManager.instance.AddOrb(lightningOrb);
                    }
                }*/
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = base.transform.position;
                fireProjectileInfo.rotation = Quaternion.LookRotation(this.punchVector);
                fireProjectileInfo.crit = base.RollCrit();
                fireProjectileInfo.damage = ((this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * HayMaker.hayMakerGritBonus));
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone");
                fireProjectileInfo.projectilePrefab.GetComponent<ProjectileProximityBeamController>().damageCoefficient = 1;
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

                this.FireSecondaryRaysServer();
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