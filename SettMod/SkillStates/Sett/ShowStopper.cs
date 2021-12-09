﻿using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class ShowStopper : BaseSkillState
    {
        public static float bonusHealthCoefficient = Modules.Config.bonusHealthCoefficient.Value;
        public static Vector3 CameraPosition = new Vector3(0f, 2f, -25f);
        public static float dodgeFOV;
        public static float dropForce = 80f;
        public static float jumpDuration = 0.6f;
        public static float slamDamageCoefficient = Modules.Config.slamDamageCoefficient.Value;
        public static float slamForce = Modules.Config.slamForce.Value;
        public static float slamProcCoefficient = 1f;
        public static float slamRadius = Modules.Config.slamRadius.Value;
        protected Animator animator;
        protected float bonusHealth;
        private bool detonateNextFrame;
        private Ray downRay;
        private Vector3 flyVector = Vector3.zero;
        private SettGrabController grabController;
        private bool hasDropped;
        private float initialTime;
        private Transform modelTransform;
        private Transform slamCenterIndicatorInstance;
        private Transform slamIndicatorInstance;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            CameraTargetParams ctp = base.cameraTargetParams;
            float denom = (1 + Time.fixedTime - this.initialTime);
            float smoothFactor = 8 / Mathf.Pow(denom, 2);
            Vector3 smoothVector = new Vector3(-3 / 20, 1 / 16, -1);
            ctp.idealLocalCameraPos = CameraPosition + smoothFactor * smoothVector;

            if (!this.hasDropped)
            {
                base.characterMotor.rootMotion += this.flyVector * (8f * EntityStates.Mage.FlyUpState.speedCoefficientCurve.Evaluate(base.fixedAge / ShowStopper.jumpDuration) * Time.fixedDeltaTime);
                base.characterMotor.velocity.y = 0f;

                this.AttemptGrab(10f);
            }

            if (base.fixedAge >= (0.25f * ShowStopper.jumpDuration) && !this.slamIndicatorInstance)
            {
                this.CreateIndicator();
            }

            if (base.fixedAge >= ShowStopper.jumpDuration && !this.hasDropped)
            {
                this.hasDropped = true;
                base.characterMotor.disableAirControlUntilCollision = true;
                base.characterMotor.velocity.y = -ShowStopper.dropForce;
                this.AttemptGrab(15f);
            }

            if (this.hasDropped && base.isAuthority && (this.detonateNextFrame || (base.characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
            {
                this.LandingImpact();
                this.outer.SetNextStateToMain();
            }

            if (base.fixedAge >= 5f && base.isAuthority && this.hasDropped)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterMotor.onMovementHit += this.OnMovementHit;
            this.bonusHealth = 0f;
            this.modelTransform = base.GetModelTransform();
            this.flyVector = Vector3.up;
            this.hasDropped = false;
            this.initialTime = Time.fixedTime;

            string[] Showstopperanim = new string[] { "ShowStopper", "ShowStopper2", "ShowStopper3" };
            System.Random random = new System.Random();
            int index = random.Next(Showstopperanim.Length);
            base.PlayCrossfade("FullBody, Override", Showstopperanim[index], "HighJump.playbackRate", ShowStopper.jumpDuration, 0.05f);

            Util.PlaySound("SettRSFX", base.gameObject);
            Util.PlaySound("SettRVO", base.gameObject);

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = base.characterMotor.velocity * 0.65f;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.grabController) this.grabController.Release();

            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.slamCenterIndicatorInstance) EntityState.Destroy(this.slamCenterIndicatorInstance.gameObject);

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (NetworkServer.active && base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.characterMotor.onMovementHit -= this.OnMovementHit;
        }

        public override void Update()
        {
            base.Update();

            if (this.slamIndicatorInstance) this.UpdateSlamIndicator();
        }

        private void AttemptGrab(float grabRadius)
        {
            if (this.grabController) return;

            Ray aimRay = base.GetAimRay();

            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = false,
                searchOrigin = base.transform.position,
                searchDirection = Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = grabRadius,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            HurtBox target = search.GetResults().FirstOrDefault<HurtBox>();
            if (target)
            {
                if (target.healthComponent && target.healthComponent.body)
                {
                    if (BodyMeetsGrabConditions(target.healthComponent.body))
                    {
                        this.bonusHealth = target.healthComponent.fullCombinedHealth;
                        this.grabController = target.healthComponent.body.gameObject.AddComponent<SettGrabController>();
                        this.grabController.pivotTransform = this.FindModelChild("R_Hand");
                    }
                    if (NetworkServer.active && !base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
                    {
                        base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                    }
                }
            }
        }

        private bool BodyMeetsGrabConditions(CharacterBody targetBody)
        {
            bool meetsConditions = true;

            //if (targetBody.hullClassification == HullClassification.BeetleQueen) meetsConditions = false;

            return meetsConditions;
        }

        private void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = base.transform.position
                };

                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.localScale = Vector3.one * ShowStopper.slamRadius;

                this.slamCenterIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamCenterIndicatorInstance.localScale = (Vector3.one * ShowStopper.slamRadius) / 3f;
            }
        }

        private void LandingImpact()
        {
            if (this.grabController) this.grabController.Release();

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.characterMotor.velocity *= 0.1f;

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = ShowStopper.slamRadius;
            blastAttack.procCoefficient = ShowStopper.slamProcCoefficient;
            blastAttack.position = base.characterBody.footPosition;
            blastAttack.attacker = base.gameObject;
            blastAttack.crit = base.RollCrit();
            blastAttack.baseDamage = (this.damageStat * ShowStopper.slamDamageCoefficient) + (ShowStopper.bonusHealthCoefficient * this.bonusHealth);
            blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
            blastAttack.baseForce = ShowStopper.slamForce;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
            blastAttack.Fire();

            Util.PlaySound("SettRImpact", base.gameObject);

            for (int i = 0; i <= 4; i += 1)
            {
                Vector3 effectPosition = base.characterBody.footPosition + (UnityEngine.Random.insideUnitSphere * 2f);
                effectPosition.y = base.characterBody.footPosition.y;
                EffectManager.SpawnEffect(EntityStates.LemurianBruiserMonster.SpawnState.spawnEffectPrefab, new EffectData
                {
                    origin = effectPosition,
                    scale = 0.25f
                }, false);
            }

            Vector3 newPosition = new Vector3(base.characterBody.footPosition.x, base.characterBody.footPosition.y + 4f, base.characterBody.footPosition.z);
            if (base.characterMotor) base.characterMotor.Motor.SetPosition(newPosition);
            if (NetworkServer.active && base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            HealthComponent healthComponent = movementHitInfo.hitCollider.transform.root.gameObject.GetComponent<HealthComponent>();
            TeamComponent teamComponent = movementHitInfo.hitCollider.transform.root.gameObject.GetComponent<TeamComponent>();
            if (healthComponent && teamComponent.teamIndex != base.GetTeam())
            {
                this.detonateNextFrame = false;
            }
            else this.detonateNextFrame = true;
        }

        private void UpdateSlamIndicator()
        {
            if (this.slamIndicatorInstance)
            {
                float maxDistance = 250f;

                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = base.transform.position
                };

                RaycastHit raycastHit;
                if (Physics.Raycast(this.downRay, out raycastHit, maxDistance, LayerIndex.world.mask))
                {
                    this.slamIndicatorInstance.transform.position = raycastHit.point;
                    this.slamIndicatorInstance.transform.up = raycastHit.normal;

                    this.slamCenterIndicatorInstance.transform.position = raycastHit.point;
                    this.slamCenterIndicatorInstance.transform.up = raycastHit.normal;
                }
            }
        }
    }
}