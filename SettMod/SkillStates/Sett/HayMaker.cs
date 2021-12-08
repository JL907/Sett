using EntityStates;
using RoR2;
using SettMod.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        public static Vector3 CameraPosition = new Vector3(1.8f, -2.4f, -8f);
        public static float hayMakerDamageCoefficient = Modules.Config.hayMakerDamageCoefficient.Value;
        public static float hayMakerForce = 1000f;
        public static float hayMakerGritBonus = Modules.Config.hayMakerGritBonus.Value;
        public static float hayMakerProcCoefficient = 1f;
        public static float hayMakerRadius = 55f;
        public GameObject blastEffectPrefab = Resources.Load<GameObject>("prefabs/effects/SonicBoomEffect");
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            if (!this.slamIndicatorInstance) this.CreateIndicator(); this.UpdateSlamIndicator();

            if (this.stopwatch >= this.startUp && !this.hasFired)
            {
                this.hasFired = true;
                Util.PlaySound("SettWVO", base.gameObject);
                if (NetworkServer.active)
                {
                    this.Fire();
                }
                for (int i = 0; i <= 20; i++)
                {
                    float coneSize = 60f;
                    Quaternion punchRot = Util.QuaternionSafeLookRotation(this.characterDirection.forward.normalized);
                    float spreadFactor = 0.01f;
                    punchRot.x += Random.Range(-spreadFactor, spreadFactor) * coneSize;
                    punchRot.y += Random.Range(-spreadFactor, spreadFactor) * coneSize;
                    EffectManager.SpawnEffect(this.blastEffectPrefab, new EffectData
                    {
                        origin = this.characterBody.corePosition,
                        scale = 100f,
                        rotation = punchRot
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
            base.StartAimMode(0.5f + this.duration, false);
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
            base.OnExit();
        }

        protected virtual void OnHitEnemyAuthority()
        {
        }

        private void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                Vector3 transformLocation = base.transform.position + base.characterDirection.forward * 13f;
                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.transform.position = transformLocation;
                this.slamIndicatorInstance.localScale = Vector3.one * 15f;
            }
        }

        private void Fire()
        {
            Ray aimRay = base.GetAimRay();
            Collider[] enemies = Physics.OverlapSphere(base.transform.position + base.characterDirection.forward * 13f, 15f);
            int num = 0;
            int num2 = 0;
            while (num < enemies.Length && num2 < 1000000f)
            {
                HealthComponent component = enemies[num].GetComponent<HealthComponent>();
                if (component)
                {
                    TeamComponent component2 = component.GetComponent<TeamComponent>();
                    bool flag = false;
                    if (component2)
                    {
                        flag = (component2.teamIndex == base.GetTeam());
                    }
                    if (!flag)
                    {
                        float _level = Mathf.Floor((base.characterBody.level - 1f) / 4f);
                        float bonus = HayMaker.hayMakerGritBonus + (_level * HayMaker.hayMakerGritBonus);

                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = (this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * bonus);
                        damageInfo.attacker = base.gameObject;
                        damageInfo.inflictor = base.gameObject;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = base.RollCrit();
                        damageInfo.procCoefficient = HayMaker.hayMakerProcCoefficient;
                        damageInfo.position = component.transform.position;
                        damageInfo.damageType = DamageType.BypassArmor;
                        component.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, component.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, component.gameObject);
                        num2++;
                    }
                }
                num++;
            }
        }

        private void UpdateSlamIndicator()
        {
            if (this.slamIndicatorInstance)
            {
                Vector3 transformLocation = base.transform.position + base.characterDirection.forward * 13f;
                this.slamIndicatorInstance.transform.position = transformLocation;
            }
        }
    }
}