using EntityStates;
using R2API;
using RoR2;
using SettMod.Modules;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        public static Vector3 CameraPosition = new Vector3(1.8f, -2.4f, -8f);
        public static float hayMakerDamageCoefficient = Modules.Config.hayMakerDamageCoefficient.Value;
        public static float hayMakerGritBonus = Modules.Config.hayMakerGritBonus.Value;
        public static float hayMakerGritBonusPer4 = Modules.Config.hayMakerGritBonusPer4.Value;
        public static float hayMakerProcCoefficient = 1f;
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
        private static GameObject areaIndicatorPrefab;
        private Transform slamIndicatorInstance;
        private Transform chargeIndicatorInstance;
        private Material slamIndicatorMaterial;
        private Material chargeIndicatorMaterial;
        private TemporaryOverlay temporaryOverlay;
        private Material glowMaterial;
        private Light glowLight;
        private SettGoldAura aura;
        private SettHandEnergy handEnergy;
        private bool fullGrit;
        private float fireFlashTimer;

        public static readonly Color goldColor = new Color(1f, 0.72f, 0.18f);
        public static readonly Color brightGoldColor = new Color(1f, 0.85f, 0.4f);
        public const float hitRadius = 15f;
        public const float reachDistance = 13f;
        private const float fireFlashDuration = 0.35f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            Ray aimRay = base.GetAimRay();
            if (base.characterDirection && aimRay.direction != Vector3.zero)
            {
                base.characterDirection.moveVector = aimRay.direction;
            }

            if (!this.slamIndicatorInstance) this.CreateIndicator();
            this.UpdateSlamIndicator();

            if (this.stopwatch >= this.startUp && !this.hasFired)
            {
                this.hasFired = true;
                this.fireFlashTimer = HayMaker.fireFlashDuration;
                if (this.handEnergy) this.handEnergy.Release(this.fullGrit);
                if (Modules.Config.voiceLines.Value) Util.PlaySound("SettWVO", base.gameObject);
                if (base.isAuthority)
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
                    }, false);
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
            if (base.isAuthority)
            {
                base.healthComponent.AddBarrierAuthority(currentGrit);
                gritComponent.AddGritAuthority(-currentGrit);
            }
            this.fullGrit = this.gritSnapShot >= this.maxGritSnapShot;
            if (this.fullGrit)
            {
                base.PlayCrossfade("Fullbody, Override", "HayMaker2", "HayMaker.playbackRate", this.duration, 0.2f);
            }
            else
            {
                base.PlayCrossfade("Fullbody, Override", "HayMaker", "HayMaker.playbackRate", this.duration, 0.2f);
            }
            Util.PlaySound("SettWSFX", base.gameObject);

            this.CreateGoldGlow();

            if (!this.slamIndicatorInstance) this.CreateIndicator();
        }

        public override void OnExit()
        {
            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            if (this.chargeIndicatorInstance) EntityState.Destroy(this.chargeIndicatorInstance.gameObject);
            if (this.slamIndicatorMaterial) EntityState.Destroy(this.slamIndicatorMaterial);
            if (this.chargeIndicatorMaterial) EntityState.Destroy(this.chargeIndicatorMaterial);
            if (this.temporaryOverlay) EntityState.Destroy(this.temporaryOverlay);
            if (this.glowMaterial) EntityState.Destroy(this.glowMaterial);
            if (this.glowLight) EntityState.Destroy(this.glowLight.gameObject);
            if (this.aura) this.aura.Stop();
            if (this.handEnergy) this.handEnergy.Stop();
            base.OnExit();
        }

        protected virtual void OnHitEnemyAuthority()
        {
        }

        private void Fire()
        {
            if (!this.slamIndicatorInstance) return;

            float _level = Mathf.Floor(base.characterBody.level / 4f);
            float bonus = HayMaker.hayMakerGritBonus + (_level * HayMaker.hayMakerGritBonusPer4);
            Vector3 center = this.slamIndicatorInstance.transform.position;
            Vector3 pushDirection = (center - base.characterBody.corePosition).normalized;

            BlastAttack blastAttack = new BlastAttack();
            blastAttack.radius = HayMaker.hitRadius;
            blastAttack.position = center;
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = base.GetTeam();
            blastAttack.baseDamage = (this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * bonus);
            blastAttack.crit = base.RollCrit();
            blastAttack.procCoefficient = HayMaker.hayMakerProcCoefficient;
            blastAttack.procChainMask = default;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.baseForce = 0f;
            blastAttack.bonusForce = pushDirection * 3000f;
            blastAttack.damageType = DamageType.BypassArmor;
            blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            if (Modules.Assets.swordHitImpactEffect) blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(Modules.Assets.swordHitImpactEffect);
            DamageAPI.AddModdedDamageType(blastAttack, SettPlugin.settDamage);

            BlastAttack.Result result = blastAttack.Fire();
            if (result.hitCount > 0)
            {
                EffectManager.SimpleSoundEffect(Modules.Assets.swordHitSoundEvent.index, center, true);
            }
        }

        public override void Update()
        {
            base.Update();
            this.UpdateGoldGlow();
        }

        private float ChargeProgress()
        {
            return this.startUp > 0f ? Mathf.Clamp01(this.stopwatch / this.startUp) : 1f;
        }

        private void CreateGoldGlow()
        {
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
                Material baseMaterial = LegacyResourcesAPI.Load<Material>("Materials/matVagrantEnergized");
                if (characterModel && baseMaterial)
                {
                    this.glowMaterial = UnityEngine.Object.Instantiate<Material>(baseMaterial);
                    HayMaker.TintMaterial(this.glowMaterial, HayMaker.goldColor, 0f);
                    this.temporaryOverlay = base.gameObject.AddComponent<TemporaryOverlay>();
                    this.temporaryOverlay.originalMaterial = this.glowMaterial;
                    this.temporaryOverlay.AddToCharacerModel(characterModel);
                }
            }

            this.aura = SettGoldAura.Create(base.characterBody, this.fullGrit);
            if (this.aura) this.aura.SetIntensity(0.6f);

            Transform anchor = this.FindModelChild("R_Hand");
            this.handEnergy = SettHandEnergy.Create(anchor);

            GameObject lightObject = new GameObject("SettHayMakerGlow");
            lightObject.transform.SetParent(anchor ? anchor : base.transform, false);
            this.glowLight = lightObject.AddComponent<Light>();
            this.glowLight.type = LightType.Point;
            this.glowLight.color = HayMaker.goldColor;
            this.glowLight.range = this.fullGrit ? 14f : 10f;
            this.glowLight.intensity = 0f;
            this.glowLight.shadows = LightShadows.None;
        }

        private void UpdateGoldGlow()
        {
            float charge = this.ChargeProgress();
            float strength = this.fullGrit ? 1.6f : 1f;
            float pulse = 1f + 0.15f * Mathf.Sin(Time.time * 18f);
            float glow = charge * charge * strength * pulse;

            if (this.fireFlashTimer > 0f)
            {
                this.fireFlashTimer -= Time.deltaTime;
                float flash = Mathf.Clamp01(this.fireFlashTimer / HayMaker.fireFlashDuration);
                glow = Mathf.Max(glow, strength * (1f + 1.5f * flash));
            }
            else if (this.hasFired)
            {
                float fadeTime = Mathf.Max(0.01f, this.EarlyExitTime - this.startUp - HayMaker.fireFlashDuration);
                float fade = 1f - Mathf.Clamp01((this.stopwatch - this.startUp - HayMaker.fireFlashDuration) / fadeTime);
                glow = strength * fade;
            }

            Color color = this.fullGrit ? HayMaker.brightGoldColor : HayMaker.goldColor;
            if (this.glowMaterial) HayMaker.TintMaterial(this.glowMaterial, color, Mathf.Clamp01(glow));
            if (this.glowLight) this.glowLight.intensity = 4f * glow;
            if (this.handEnergy) this.handEnergy.SetCharge(charge);
            if (this.aura) this.aura.SetIntensity(Mathf.Max(0.6f * (this.hasFired ? glow : 1f), glow));

            if (this.chargeIndicatorMaterial)
            {
                HayMaker.TintMaterial(this.chargeIndicatorMaterial, color, this.hasFired ? Mathf.Clamp01(glow) : 0.35f + 0.5f * charge);
            }
            if (this.slamIndicatorMaterial)
            {
                HayMaker.TintMaterial(this.slamIndicatorMaterial, color, this.hasFired ? Mathf.Clamp01(glow) * 0.5f : 0.25f + 0.15f * pulse * charge);
            }
        }

        private static void TintMaterial(Material material, Color color, float alpha)
        {
            Color tinted = new Color(color.r, color.g, color.b, alpha);
            if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", tinted);
            if (material.HasProperty("_Color")) material.SetColor("_Color", tinted);
        }

        private Material TintIndicator(Transform indicator)
        {
            Material shared = null;
            foreach (Renderer renderer in indicator.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.sharedMaterial) continue;
                if (!shared) shared = UnityEngine.Object.Instantiate<Material>(renderer.sharedMaterial);
                renderer.sharedMaterial = shared;
            }
            if (shared) HayMaker.TintMaterial(shared, HayMaker.goldColor, 0.3f);
            return shared;
        }

        private Vector3 GetIndicatorPoint()
        {
            Ray aimRay = base.GetAimRay();
            Transform hand = this.FindModelChild("R_Hand");
            if (hand) aimRay.origin = hand.position;
            return aimRay.GetPoint(HayMaker.reachDistance);
        }

        internal static GameObject GetAreaIndicatorPrefab()
        {
            if (HayMaker.areaIndicatorPrefab) return HayMaker.areaIndicatorPrefab;
            HayMaker.areaIndicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Characters/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();
            if (!HayMaker.areaIndicatorPrefab) HayMaker.areaIndicatorPrefab = EntityStates.Huntress.ArrowRain.areaIndicatorPrefab;
            return HayMaker.areaIndicatorPrefab;
        }

        private void CreateIndicator()
        {
            GameObject prefab = HayMaker.GetAreaIndicatorPrefab();
            if (!prefab) return;

            Vector3 point = this.GetIndicatorPoint();

            this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(prefab).transform;
            this.slamIndicatorInstance.localScale = Vector3.one * HayMaker.hitRadius;
            this.slamIndicatorInstance.position = point;
            this.slamIndicatorMaterial = this.TintIndicator(this.slamIndicatorInstance);

            this.chargeIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(prefab).transform;
            this.chargeIndicatorInstance.localScale = Vector3.zero;
            this.chargeIndicatorInstance.position = point;
            this.chargeIndicatorMaterial = this.TintIndicator(this.chargeIndicatorInstance);
        }

        private void UpdateSlamIndicator()
        {
            if (!this.slamIndicatorInstance) return;

            Vector3 point = this.GetIndicatorPoint();
            this.slamIndicatorInstance.position = point;

            if (this.chargeIndicatorInstance)
            {
                float charge = this.ChargeProgress();
                float eased = 1f - (1f - charge) * (1f - charge);
                this.chargeIndicatorInstance.position = point;
                this.chargeIndicatorInstance.localScale = Vector3.one * (HayMaker.hitRadius * eased);
            }
        }
    }
}