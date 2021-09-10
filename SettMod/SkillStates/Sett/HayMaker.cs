using EntityStates;
using RoR2;
using SettMod.Modules;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class HayMaker : BaseSkillState
    {
        protected float startUp = 0.8f;
        protected float EarlyExitTime = 1.2f;
        protected float baseDuration = 3.55f;
        public static float hayMakerRadius = 55f;
        public static float hayMakerDamageCoefficient = 14f;
        public static float hayMakerProcCoefficient = 1f;
        public static float hayMakerGritBonus = 3f;
        public static float hayMakerForce = 1000f;
        public GameObject blastEffectPrefab = Resources.Load<GameObject>("prefabs/effects/SonicBoomEffect");
        private float gritSnapShot;
        private Ray downRay;
        private Vector3 hitSphereScale = new Vector3(50f, 14f, 14f);
        private Transform slamIndicatorInstance;
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
            base.StartAimMode(0.5f + this.duration, false);
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
            if (!this.slamIndicatorInstance) this.CreateIndicator();

        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            base.OnExit();
        }


        protected virtual void OnHitEnemyAuthority()
        {

        }

        private void UpdateSlamIndicator()
        {
            if (this.slamIndicatorInstance)
            {
                float maxDistance = 250f;

                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = Vector3.up + base.transform.position + base.characterDirection.forward * 23f
                };

                RaycastHit raycastHit;
                if (Physics.Raycast(this.downRay, out raycastHit, maxDistance, LayerIndex.world.mask))
                {
                    this.slamIndicatorInstance.transform.position = raycastHit.point;
                    this.slamIndicatorInstance.transform.up = raycastHit.normal;
                }
            }
        }

        private void CreateIndicator()
        {
            if (EntityStates.Huntress.ArrowRain.areaIndicatorPrefab)
            {
                this.downRay = new Ray
                {
                    direction = Vector3.down,
                    origin = Vector3.up + base.transform.position + base.characterDirection.forward * 23f
                };
                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.localScale = Vector3.one * 25f;
            }
        }


        private void Fire()
        {
            Ray aimRay = base.GetAimRay();
            Collider[] enemies = Physics.OverlapSphere(base.transform.position + base.characterDirection.forward * 23f, this.hitSphereScale.x / 2);
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
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = (this.damageStat * HayMaker.hayMakerDamageCoefficient) + (this.gritSnapShot * HayMaker.hayMakerGritBonus);
                        damageInfo.attacker = base.gameObject;
                        damageInfo.inflictor = base.gameObject;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = base.RollCrit();
                        damageInfo.procCoefficient = HayMaker.hayMakerDamageCoefficient;
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if (!this.slamIndicatorInstance) this.CreateIndicator();

            this.UpdateSlamIndicator();

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