using EntityStates;
using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class Facebreaker : BaseSkillState
    {
        protected float damageCoefficient = 6f;

        protected float startUp = 0.5f;
        protected float baseDuration = 0.80f;
        public float duration;

        private OverlapAttack attack;

        private bool hasFired;
        protected Animator animator;
        protected float stopwatch;
        protected string hitboxName = "FaceBreaker";

        private Transform slamIndicatorInstance;
        //private Transform slamCenterIndicatorInstance;
        private Ray downRay;
        public GameObject blastEffectPrefab = Resources.Load<GameObject>("prefabs/effects/SonicBoomEffect");
        protected NetworkSoundEventIndex impactSound;

        public Transform pullOrigin;
        public AnimationCurve pullStrengthCurve;
        public int maximumPullCount = int.MaxValue;
        private List<CharacterBody> pullList = new List<CharacterBody>();
        private bool pulling;

        public static Vector3 CameraPosition = new Vector3(0f, -2.4f, -25f);
        private float initialTime;

        private bool front;
        private bool back;

        public static float pullRadius = Modules.Config.faceBreakerPullRadius.Value;
        public static float pullForce = Modules.Config.faceBreakerPullForce.Value;

        public override void OnEnter()
        {
            base.OnEnter();
            this.damageCoefficient = Modules.Config.faceBreakerDamageCoefficient.Value;
            base.characterBody.SetAimTimer(2f);
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.hasFired = false;
            this.animator = base.GetModelAnimator();

            base.PlayCrossfade("Fullbody, Override", "Facebreaker_Start", "FaceBreakerStartUp.playbackRate", this.startUp, 0.05f);

            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = base.GetModelTransform();
            hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == this.hitboxName);

            this.pullStrengthCurve = AnimationCurve.EaseInOut(0.1f, 0f, 1f, 1f);


            Util.PlaySound("SettESFX", base.gameObject);

            this.impactSound = Modules.Assets.swordHitSoundEvent.index;

            this.attack = CreateAttack(hitBoxGroup);

            this.CreateIndicator();
        }

        public override void Update()
        {
            base.Update();

            if (this.slamIndicatorInstance) this.UpdateSlamIndicator();
        }

        public override void OnExit()
        {
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.slamIndicatorInstance) EntityState.Destroy(this.slamIndicatorInstance.gameObject);
            //if (this.slamCenterIndicatorInstance) EntityState.Destroy(this.slamCenterIndicatorInstance.gameObject);
            base.OnExit();
        }

        private void InitializePull()
        {
            if (this.pulling)
            {
                return;
            }
            this.pulling = true;
            Collider[] array = Physics.OverlapSphere(((this.pullOrigin) ? this.pullOrigin.position : base.transform.position), Facebreaker.pullRadius);
            int num = 0;
            int num2 = 0;
            while (num < array.Length && num2 < this.maximumPullCount)
            {
                HealthComponent component = array[num].GetComponent<HealthComponent>();
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
                        this.AddToPullList(component.gameObject);
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

                    //this.slamCenterIndicatorInstance.transform.position = raycastHit.point;
                    //this.slamCenterIndicatorInstance.transform.up = raycastHit.normal;
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
                    origin = base.transform.position
                };

                this.slamIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                this.slamIndicatorInstance.localScale = Vector3.one * Facebreaker.pullRadius;


                for (int i = 0; i <= 18; i += 1)
                {
                    Vector3 effectPosition = base.characterBody.footPosition + (UnityEngine.Random.insideUnitSphere * Facebreaker.pullRadius);
                    Vector3 _direction = (base.characterBody.footPosition - effectPosition).normalized;
                    effectPosition.y = base.characterBody.footPosition.y;
                    EffectManager.SpawnEffect(this.blastEffectPrefab, new EffectData
                    {
                        origin = effectPosition,
                        scale = 1f * Facebreaker.pullRadius,
                        rotation = Quaternion.LookRotation(_direction)
                    }, false);
                }

                //this.slamCenterIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(EntityStates.Huntress.ArrowRain.areaIndicatorPrefab).transform;
                //this.slamCenterIndicatorInstance.localScale = (Vector3.one * Facebreaker.pullRadius) / 3f;
            }
        }

        private void AddToPullList(GameObject affectedObject)
        {
            CharacterBody component = affectedObject.GetComponent<CharacterBody>();
            if (!this.pullList.Contains(component))
            {
                this.pullList.Add(component);
            }
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound("Hit", base.gameObject);
        }

        private void PullEnemies(float deltaTime)
        {
            if (!this.pulling)
            {
                this.InitializePull();
            }
            this.front = false;
            this.back = false;
            for (int i = 0; i < this.pullList.Count; i++)
            {
                CharacterBody characterBody = this.pullList[i];
                if (characterBody && characterBody.transform)
                {
                    Vector3 vector = ((this.pullOrigin) ? this.pullOrigin.position : base.transform.position) - characterBody.corePosition;
                    float d = this.pullStrengthCurve.Evaluate(vector.magnitude / Facebreaker.pullRadius);
                    Vector3 b = vector.normalized * d * deltaTime * Facebreaker.pullForce;
                    CharacterMotor component = characterBody.GetComponent<CharacterMotor>();
                    Vector3 forward = this.transform.TransformDirection(Vector3.forward);
                    Vector3 toEnemy = characterBody.transform.position - base.transform.position;
                    if (component)
                    {
                        component.rootMotion += b;
                        if (component.useGravity)
                        {
                            component.rootMotion.y -= (Physics.gravity.y * deltaTime * d);
                            if (Vector3.Dot(forward, toEnemy) < 0) back = true;
                            else if (Vector3.Dot(forward, toEnemy) > 0) front = true;
                            else if (Vector3.Dot(forward, toEnemy) == 0) front = true;
                        }
                    }
                    else
                    {
                        Rigidbody component2 = characterBody.GetComponent<Rigidbody>();
                        if (component2)
                        {
                            component2.velocity += b;
                            if (Vector3.Dot(forward, toEnemy) < 0) back = true;
                            else if (Vector3.Dot(forward, toEnemy) > 0) front = true;
                            else if (Vector3.Dot(forward, toEnemy) == 0) front = true;
                        }
                    }
                }
            }
        }

        protected OverlapAttack CreateAttack(HitBoxGroup hitBoxGroup)
        {
            var attack = new OverlapAttack();
            attack.damageType = DamageType.Stun1s | DamageType.SlowOnHit;
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = base.GetTeam();
            attack.damage = this.damageCoefficient * this.damageStat;
            attack.procCoefficient = 1f;
            attack.hitEffectPrefab = Modules.Assets.swordHitImpactEffect;
            attack.forceVector = Vector3.zero;
            attack.pushAwayForce = 0f;
            attack.hitBoxGroup = hitBoxGroup;
            attack.isCrit = base.RollCrit();
            attack.impactSound = this.impactSound;
            return attack;
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            CameraTargetParams ctp = base.cameraTargetParams;
            float denom = (1 + Time.fixedTime - this.initialTime);
            float smoothFactor = 8 / Mathf.Pow(denom, 2);
            Vector3 smoothVector = new Vector3(-3 / 20, 1 / 16, -1);
            ctp.idealLocalCameraPos = CameraPosition + smoothFactor * smoothVector;

            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.startUp)
            {
                if (!this.hasFired)
                {
                    this.hasFired = true;

                    Util.PlaySound("SettEVO", base.gameObject);

                    string clip = "";
                    if (front && back)
                    {
                        clip = "Facebreaker_Both";
                    }
                    else if (back && !front)
                    {
                        clip = "Facebreaker_Back";
                    }
                    else if (front && !back)
                    {
                        clip = "Facebreaker_Front";
                    }
                    else if (!front && !back)
                    {
                        clip = "Facebreaker_Miss";
                    }
                    base.PlayCrossfade("Fullbody, Override", clip, "FaceBreaker.playbackRate", this.duration, 0.05f);
                }
            }

            if (this.stopwatch <= this.duration) this.PullEnemies(Time.fixedDeltaTime);

            if (this.stopwatch >= (this.duration * this.startUp))
            {
                if (base.isAuthority)
                {
                    if (this.attack.Fire())
                    {
                        this.OnHitEnemyAuthority();
                    }
                }

            }


            if (this.stopwatch >= this.duration && base.isAuthority && this.hasFired)
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