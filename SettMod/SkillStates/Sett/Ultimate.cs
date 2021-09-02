
using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class ShowStopper : BaseSkillState
    {
        public static float jumpDuration = 0.6f;
        public static float dropForce = 80f;

        public static float slamRadius = 25f;
        public static float slamDamageCoefficient = 24f;
        public static float slamProcCoefficient = 1f;
        public static float slamForce = 2000f;

        public static float dodgeFOV;

        protected float bonusHealth;
        private bool hasDropped;
        private Vector3 flyVector = Vector3.zero;
        private Transform modelTransform;
        private Transform slamIndicatorInstance;
        private Transform slamCenterIndicatorInstance;
        private Ray downRay;
        private SettGrabController grabController;


        protected Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.bonusHealth = 0f;
            this.modelTransform = base.GetModelTransform();
            this.flyVector = Vector3.up;
            this.hasDropped = false;

            base.PlayAnimation("FullBody, Override", "ShowStopper", "HighJump.playbackRate", ShowStopper.jumpDuration);
            Util.PlaySound("SettRSFX", base.gameObject);
            Util.PlaySound("SettRVO", base.gameObject);

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = Vector3.zero;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(Modules.Buffs.armorBuff, 1f * ShowStopper.jumpDuration);
                base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 1f * ShowStopper.jumpDuration);
            }
        }

        public override void Update()
        {
            base.Update();

            if (this.slamIndicatorInstance) this.UpdateSlamIndicator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!this.hasDropped)
            {
                base.characterMotor.rootMotion += this.flyVector * ((0.6f * (this.moveSpeedStat)) * EntityStates.Mage.FlyUpState.speedCoefficientCurve.Evaluate(base.fixedAge / ShowStopper.jumpDuration) * Time.fixedDeltaTime);
                base.characterMotor.velocity.y = 0f;

                this.AttemptGrab(10f);
            }

            if (base.fixedAge >= (0.25f * ShowStopper.jumpDuration) && !this.slamIndicatorInstance)
            {

                if (base.cameraTargetParams)
                {
                    base.cameraTargetParams.fovOverride = Mathf.Lerp(60f, 90f, base.fixedAge / ShowStopper.jumpDuration);
                }
                this.CreateIndicator();
            }

            if (base.fixedAge >= ShowStopper.jumpDuration && !this.hasDropped)
            {
                this.hasDropped = true;

                base.characterMotor.disableAirControlUntilCollision = true;
                base.characterMotor.velocity.y = -ShowStopper.dropForce;

                //base.PlayAnimation("FullBody, Override", "ShowStopperSlam", "HighJump.playbackRate", 0.2f);
                this.AttemptGrab(15f);
            }

            if (this.hasDropped && base.isAuthority && !base.characterMotor.disableAirControlUntilCollision)
            {
                this.LandingImpact();
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.fixedAge >= ShowStopper.jumpDuration + 2f && this.hasDropped && base.isAuthority)
            {
                this.LandingImpact();
                this.outer.SetNextStateToMain();
            }
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

            base.characterMotor.velocity *= 0.1f;

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = ShowStopper.slamRadius;
            blastAttack.procCoefficient = ShowStopper.slamProcCoefficient;
            blastAttack.position = base.characterBody.footPosition;
            blastAttack.attacker = base.gameObject;
            blastAttack.crit = base.RollCrit();
            blastAttack.baseDamage = (base.characterBody.damage * ShowStopper.slamDamageCoefficient) + (0.1f * this.bonusHealth);
            blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            blastAttack.baseForce = ShowStopper.slamForce;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
            blastAttack.Fire();

            base.gameObject.transform.position += new Vector3(0, 3, 0);

            Util.PlaySound("SettRImpact", base.gameObject);

            for (int i = 0; i <= 4; i += 1)
            {
                Vector3 effectPosition = base.characterBody.footPosition + (UnityEngine.Random.insideUnitSphere * 2f);
                effectPosition.y = base.characterBody.footPosition.y;
                EffectManager.SpawnEffect(EntityStates.LemurianBruiserMonster.SpawnState.spawnEffectPrefab, new EffectData
                {
                    origin = effectPosition,
                    scale = 0.5f
                }, true);
            }
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

        public override void OnExit()
        {

            if (base.cameraTargetParams)
            {
                base.cameraTargetParams.fovOverride = -1f;
            }

            if (this.grabController) this.grabController.Release();

            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.slamCenterIndicatorInstance) EntityState.Destroy(this.slamCenterIndicatorInstance.gameObject);

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (NetworkServer.active && base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.OnExit();
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
                        this.grabController.pivotTransform = this.FindModelChild("L_Hand");
                    }

                    if (NetworkServer.active)
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}