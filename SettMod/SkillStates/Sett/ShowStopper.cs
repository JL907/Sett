using EntityStates;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class ShowStopper : BaseSkillState
    {
        public static float bonusHealthCoefficient = Modules.Config.bonusHealthCoefficient.Value;
        public static Vector3 CameraPosition = new Vector3(0f, 3f, -25f);
        public static float dodgeFOV;
        public static float dropForce = 80f;
        public static float jumpDuration = 0.6f;
        public static float slamDamageCoefficient = Modules.Config.slamDamageCoefficient.Value;
        public static float slamForce = Modules.Config.slamForce.Value;
        public static float slamProcCoefficient = 1f;
        public static float slamRadius = Modules.Config.slamRadius.Value;
        protected Animator animator;
        protected float bonusDamage = 0f;
        private bool detonateNextFrame;
        private Ray downRay;
        private Vector3 flyVector = Vector3.zero;
        private SettGrabController grabController;
        private bool hasDropped;
        private float initialTime;
        private Transform modelTransform;
        private Transform slamCenterIndicatorInstance;
        private Transform slamIndicatorInstance;
        private CameraTargetParams.CameraParamsOverrideHandle handle;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            CameraTargetParams ctp = base.cameraTargetParams;
            CharacterCameraParamsData characterCameraParamsData = ctp.currentCameraParamsData;
            float denom = (1 + Time.fixedTime - this.initialTime);
            float smoothFactor = 8 / Mathf.Pow(denom, 2);
            Vector3 smoothVector = new Vector3(0, 0, -1);
            characterCameraParamsData.idealLocalCameraPos = CameraPosition + smoothFactor * smoothVector;

            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = characterCameraParamsData,
                priority = 0,
            };
            

            handle = ctp.AddParamsOverride(request);
            base.cameraTargetParams.RemoveParamsOverride(handle);

            if (!this.hasDropped)
            {
                base.characterMotor.rootMotion += this.flyVector * (8f * EntityStates.Mage.FlyUpState.speedCoefficientCurve.Evaluate(base.fixedAge / ShowStopper.jumpDuration) * Time.fixedDeltaTime);
                base.characterMotor.velocity.y = 0f;

                if (base.isAuthority) this.AttemptGrab(10f);
            }

            if (base.fixedAge >= (0.25f * ShowStopper.jumpDuration) && !this.slamIndicatorInstance)
            {
                this.CreateIndicator();
            }
            if (base.fixedAge >= ShowStopper.jumpDuration && !this.hasDropped)
            {
                this.StartDrop();
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
            this.modelTransform = base.GetModelTransform();
            this.flyVector = Vector3.up;
            this.hasDropped = false;
            this.initialTime = Time.fixedTime;

            string[] Showstopperanim = new string[] { "ShowStopper", "ShowStopper2", "ShowStopper3" };
            System.Random random = new System.Random();
            int index = random.Next(Showstopperanim.Length);
            base.PlayCrossfade("FullBody, Override", Showstopperanim[index], "HighJump.playbackRate", ShowStopper.jumpDuration, 0.05f);

            Util.PlaySound("SettRSFX", base.gameObject);
            if (Modules.Config.voiceLines.Value) Util.PlaySound("SettRVO", base.gameObject);

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = base.characterMotor.velocity * 0.65f;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.ReleaseGrab();

            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.slamCenterIndicatorInstance) EntityState.Destroy(this.slamCenterIndicatorInstance.gameObject);

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (NetworkServer.active && base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.characterMotor.onMovementHit -= this.OnMovementHit;
            base.cameraTargetParams.RemoveParamsOverride(handle);
        }

        public void StartDrop()
        {
            this.hasDropped = true;
            base.characterMotor.disableAirControlUntilCollision = true;
            base.characterMotor.velocity.y = -ShowStopper.dropForce;
            if (base.isAuthority) this.AttemptGrab(15f);
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

            List<HurtBox> sortedCandidates = search.GetResults().OrderByDescending(candidate => GetHurtBoxFullCombinedHealth(candidate)).ToList();

            HurtBox target = sortedCandidates.FirstOrDefault();
            if (target)
            {
                if (target.healthComponent && target.healthComponent.body)
                {
                    GameObject targetObject = target.healthComponent.body.gameObject;
                    NetworkIdentity targetIdentity = targetObject.GetComponent<NetworkIdentity>();
                    NetworkIdentity selfIdentity = base.gameObject.GetComponent<NetworkIdentity>();

                    if (BodyMeetsGrabConditions(target.healthComponent.body) && targetIdentity && selfIdentity)
                    {
                        this.bonusDamage = target.healthComponent.fullCombinedHealth * ShowStopper.bonusHealthCoefficient;

                        this.grabController = SettGrabController.Apply(base.gameObject, targetObject);
                        new SyncSettGrab(selfIdentity.netId, targetIdentity.netId).Send(NetworkDestination.Clients | NetworkDestination.Server);
                    }
                }
            }
        }

        private void ReleaseGrab()
        {
            if (!this.grabController) return;

            NetworkIdentity targetIdentity = this.grabController.GetComponent<NetworkIdentity>();
            this.grabController.Release();
            this.grabController = null;

            if (base.isAuthority && targetIdentity)
            {
                new SyncSettRelease(targetIdentity.netId).Send(NetworkDestination.Clients | NetworkDestination.Server);
            }
        }

        private float GetHurtBoxFullCombinedHealth(HurtBox hurtBox)
        {
            return hurtBox.healthComponent.fullCombinedHealth;
        }

        private bool BodyMeetsGrabConditions(CharacterBody targetBody)
        {
            bool meetsConditions = true;

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
            this.ReleaseGrab();

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.characterMotor.velocity *= 0.1f;

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = ShowStopper.slamRadius;
            blastAttack.procCoefficient = ShowStopper.slamProcCoefficient;
            blastAttack.position = base.characterBody.footPosition;
            blastAttack.procChainMask = default;
            blastAttack.attacker = base.gameObject;
            blastAttack.crit = base.RollCrit();
            blastAttack.baseDamage = this.damageStat * ShowStopper.slamDamageCoefficient;
            blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
            blastAttack.baseForce = ShowStopper.slamForce;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
            blastAttack.damageType = DamageType.Stun1s;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            DamageAPI.AddModdedDamageType(blastAttack, SettPlugin.settDamage);
            blastAttack.Fire();

            if (this.bonusDamage > 0f)
            {
                BlastAttack bonusAttack = new BlastAttack();
                bonusAttack.radius = ShowStopper.slamRadius;
                bonusAttack.procCoefficient = 0f;
                bonusAttack.position = base.characterBody.footPosition;
                bonusAttack.procChainMask = default;
                bonusAttack.attacker = base.gameObject;
                bonusAttack.crit = false;
                bonusAttack.baseDamage = this.bonusDamage;
                bonusAttack.falloffModel = BlastAttack.FalloffModel.None;
                bonusAttack.baseForce = 0f;
                bonusAttack.teamIndex = blastAttack.teamIndex;
                bonusAttack.damageType = DamageType.BypassArmor;
                bonusAttack.damageColorIndex = DamageColorIndex.Item;
                bonusAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                bonusAttack.Fire();
            }

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
            this.detonateNextFrame = true;
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