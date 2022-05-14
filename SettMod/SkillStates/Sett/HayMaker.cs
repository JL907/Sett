using EntityStates;
using R2API;
using RoR2;
using SettMod.Modules;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        public static Vector3 CameraPosition = new Vector3(1.8f, -2.4f, -8f);
        public static float hayMakerDamageCoefficient = Modules.Config.hayMakerDamageCoefficient.Value;
        public static float hayMakerForce = 1000f;
        public static float hayMakerGritBonus = Modules.Config.hayMakerGritBonus.Value;
        public static float hayMakerGritBonusPer4 = Modules.Config.hayMakerGritBonusPer4.Value;
        public static float hayMakerProcCoefficient = 1f;
        public static float hayMakerRadius = 55f;
        public GameObject blastEffectPrefab = Addressables.LoadAssetAsync<GameObject>(key: "RoR2/Junk/Treebot/SonicBoomEffect.prefab").WaitForCompletion();
        public float duration;
        protected Animator animator;
        protected float baseDuration = 1.51f;
        protected float EarlyExitTime = 1.2f;
        protected float startUp = 0.78f;
        protected float stopwatch;
        private float gritSnapShot;
        private bool hasFired;
        private float maxGritSnapShot;
        private Transform slamIndicatorInstance;
        private TemporaryOverlay temporaryOverlay;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            Ray aimRay = base.GetAimRay();
            if (base.characterDirection && aimRay.direction != Vector3.zero)
            {
                base.characterDirection.moveVector = aimRay.direction;
            }

            if (!this.slamIndicatorInstance) this.CreateIndicator(); this.UpdateSlamIndicator();

            if (this.stopwatch >= this.startUp && !this.hasFired)
            {
                this.hasFired = true;
                if (Modules.Config.voiceLines.Value) Util.PlaySound("SettWVO", base.gameObject);
                if (NetworkServer.active)
                {
                    this.Fire();
                }
                for (int i = 0; i <= 25; i++)
                {
                    float coneSize = 45f;
                    Vector3 vector = Util.ApplySpread(aimRay.direction, 10f, coneSize, 1f, 1f, 0f, 0f);
                    EffectManager.SpawnEffect(this.blastEffectPrefab, new EffectData
                    {
                        origin = this.characterBody.corePosition,
                        scale = 100f,
                        rotation = Util.QuaternionSafeLookRotation(vector)
                    }, false); ;
                }
            }

            if (this.stopwatch >= this.EarlyExitTime && base.isAuthority && this.hasFired)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }


        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(baseDuration, false);
            this.animator = base.GetModelAnimator();
            this.hasFired = false;
            this.duration = this.baseDuration;
            base.characterMotor.velocity = Vector3.zero;
            GritComponent gritComponent = base.GetComponent<GritComponent>();
            float currentGrit = gritComponent.GetCurrentGrit();
            float maxGrit = gritComponent.GetMaxGrit();
            this.gritSnapShot = currentGrit;
            this.maxGritSnapShot = maxGrit;
            base.healthComponent.AddBarrierAuthority(currentGrit);
            base.GetComponent<GritComponent>().AddGritAuthority(-currentGrit);
            if (this.gritSnapShot >= this.maxGritSnapShot)
            {
                base.PlayCrossfade("Fullbody, Override", "HayMaker2", "HayMaker.playbackRate", this.duration, 0.2f);

                Transform modelTransform = base.GetModelTransform();
                if (modelTransform)
                {
                    CharacterModel component = modelTransform.GetComponent<CharacterModel>();
                    if (component)
                    {
                        this.temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
                        Material material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<Material>("Materials/matVagrantEnergized"));
                        this.temporaryOverlay.originalMaterial = material;
                        this.temporaryOverlay.AddToCharacerModel(component);
                    }
                }
            }
            else
            {
                base.PlayCrossfade("Fullbody, Override", "HayMaker", "HayMaker.playbackRate", this.duration, 0.2f);
            }
            Util.PlaySound("SettWSFX", base.gameObject);

            if (!this.slamIndicatorInstance) this.CreateIndicator();
        }


        public override void OnExit()
        {
            //base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.temporaryOverlay)
            {
                EntityState.Destroy(this.temporaryOverlay);
            }
            base.OnExit();
        }

        protected virtual void OnHitEnemyAuthority()
        {
        }

        private void Fire()
        {
            List<HurtBox> HurtBoxes = new List<HurtBox>();
            HurtBoxes = new SphereSearch
            {
                radius = 15f,
                mask = LayerIndex.entityPrecise.mask,
                origin = this.slamIndicatorInstance.transform.position
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.teamComponent.teamIndex)).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes().ToList();

            foreach (HurtBox hurtbox in HurtBoxes)
            {
                float _level = Mathf.Floor(base.characterBody.level / 4f);
                float bonus = HayMaker.hayMakerGritBonus + (_level * HayMaker.hayMakerGritBonusPer4);

                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = (this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * bonus);
                damageInfo.attacker = base.gameObject;
                damageInfo.inflictor = base.gameObject;
                damageInfo.force = Vector3.zero;
                damageInfo.crit = base.RollCrit();
                damageInfo.procCoefficient = HayMaker.hayMakerProcCoefficient;
                damageInfo.position = hurtbox.gameObject.transform.position;
                damageInfo.damageType = DamageType.BypassArmor;
                DamageAPI.AddModdedDamageType(damageInfo, SettPlugin.settDamage);
                hurtbox.healthComponent.TakeDamage(damageInfo);

                GameObject hitEffectPrefab = Modules.Assets.swordHitImpactEffect;
                if (hitEffectPrefab)
                {
                    EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
                    {
                        origin = hurtbox.healthComponent.gameObject.transform.position,
                        rotation = Quaternion.identity,
                        networkSoundEventIndex = Modules.Assets.swordHitSoundEvent.index
                    }, true);
                }

                GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtbox.healthComponent.gameObject);
                GlobalEventManager.instance.OnHitAll(damageInfo, hurtbox.healthComponent.gameObject);
            }
        }

        private void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                float num = 13f;
                Ray aimRay = base.GetAimRay();
                aimRay.origin = this.FindModelChild("R_Hand").position;
                Vector3 point = aimRay.GetPoint(num);
                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.localScale = Vector3.one * 15f;
                this.slamIndicatorInstance.transform.position = point;
            }
        }
        private void UpdateSlamIndicator()
        {
            if (this.slamIndicatorInstance)
            {
                float num = 13f;
                Ray aimRay = base.GetAimRay();
                aimRay.origin = this.FindModelChild("R_Hand").position;
                Vector3 point = aimRay.GetPoint(num);
                this.slamIndicatorInstance.transform.position = point;
            }
        }
    }
}