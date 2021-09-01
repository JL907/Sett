using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2.Audio;

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
        private Vector3 punchVector

        {
            get
            {
                return base.characterDirection.forward.normalized;
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

        private void FireSecondaryRaysServer()
        {
            Ray aimRay = base.GetAimRay();
            TeamIndex team = base.GetTeam();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(team);
            bullseyeSearch.maxAngleFilter = 60f;
            bullseyeSearch.maxDistanceFilter = 20f;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = this.punchVector;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.RefreshCandidates();


            List<HurtBox> list = bullseyeSearch.GetResults().Where(new Func<HurtBox, bool>(Util.IsValid)).ToList<HurtBox>();
            Transform transform = base.FindModelChild("SwingCenter");

            if (transform)
            {
                for (int i = 0; i < Mathf.Min(list.Count, 100f); i++)
                {
                    HurtBox hurtBox = list[i];
                    if (hurtBox)
                    {
                        LightningOrb lightningOrb = new LightningOrb();
                        lightningOrb.bouncedObjects = new List<HealthComponent>();
                        lightningOrb.attacker = base.gameObject;
                        lightningOrb.teamIndex = team;
                        lightningOrb.damageValue = this.damageStat * 10f;
                        lightningOrb.isCrit = base.RollCrit();
                        lightningOrb.origin = hurtBox.healthComponent.body.transform.position;
                        lightningOrb.bouncesRemaining = 0;
                        lightningOrb.lightningType = LightningOrb.LightningType.Loader;
                        lightningOrb.procCoefficient = 1f;
                        lightningOrb.target = hurtBox;
                        OrbManager.instance.AddOrb(lightningOrb);
                    }

                }
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = base.GetAimRay().origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(this.punchVector);
                fireProjectileInfo.crit = base.RollCrit();
                fireProjectileInfo.damage = 0f;
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/LoaderZapCone");
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if(this.stopwatch >= this.startUp && !this.hasFired && base.isAuthority)
            {
                this.hasFired = true;
                Util.PlaySound("SettWVO", base.gameObject);

                this.FireSecondaryRaysServer();

                Vector3 effectPosition = base.characterBody.footPosition + (UnityEngine.Random.insideUnitSphere * 2f);
                effectPosition.y = base.characterBody.footPosition.y;
                EffectManager.SpawnEffect(EntityStates.LemurianBruiserMonster.SpawnState.spawnEffectPrefab, new EffectData
                {
                    origin = effectPosition,
                    scale = 1f
                }, true);
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