using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class Facebreaker2 : BaseSkillState
    {
        protected float startUp = 0.5f;
        protected float baseDuration = 0.80f;
        public float duration;

        public static float slamRadius = 2f;
        public static float slamDamageCoefficient = 8f;
        public static float slamProcCoefficient = 1f;
        public static float slamForce = 10f;

        private bool hasFired;
        protected Animator animator;

        private SettGrabController LgrabController;
        private SettGrabController RgrabController;
        protected float stopwatch;


        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.hasFired = false;
            this.duration = this.baseDuration;



            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = Vector3.zero;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();


            base.PlayAnimation("Fullbody, Override", "Facebreaker_Start", "FaceBreakerStartUp.playbackRate", this.startUp);
            Util.PlaySound("SettESFX", base.gameObject);


        }
        public override void OnExit()
        {

            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.LgrabController) this.LgrabController.Release();
            if (this.RgrabController) this.RgrabController.Release();


            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            if (NetworkServer.active && base.characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.OnExit();
        }


        protected virtual void OnHitEnemyAuthority()
        {

        }

        private void AttemptGrab(float grabRadius)
        {
            if (this.LgrabController || this.RgrabController) return;

            Ray aimRay = base.GetAimRay();

            BullseyeSearch searchL = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = true,
                searchOrigin = base.transform.position,
                searchDirection = base.inputBank.aimDirection,
                sortMode = BullseyeSearch.SortMode.DistanceAndAngle,
                maxDistanceFilter = grabRadius,
                maxAngleFilter = 45f
            };

            BullseyeSearch searchR = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = true,
                searchOrigin = base.transform.position,
                searchDirection = -base.inputBank.aimDirection,
                sortMode = BullseyeSearch.SortMode.DistanceAndAngle,
                maxDistanceFilter = grabRadius,
                maxAngleFilter = 45f
            };

            searchL.RefreshCandidates();
            searchL.FilterOutGameObject(base.gameObject);

            searchR.RefreshCandidates();
            searchR.FilterOutGameObject(base.gameObject);

            HurtBox targetL = searchL.GetResults().FirstOrDefault<HurtBox>();

            HurtBox targetR = searchR.GetResults().FirstOrDefault<HurtBox>();

            float num = Mathf.Cos(60f * 0.5f * 0.017453292f);
            if (targetL)
            {
                if (targetL.healthComponent && targetL.healthComponent.body)
                {
                    if (BodyMeetsGrabConditions(targetL.healthComponent.body))
                    {

                        this.LgrabController = targetL.healthComponent.body.gameObject.AddComponent<SettGrabController>();
                        this.LgrabController.pivotTransform = this.FindModelChild("L_Hand");
                        base.characterMotor.disableAirControlUntilCollision = false;
                    }

                    if (NetworkServer.active)
                    {
                        //base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                    }
                }
            }
            if (targetR)
            {
                if (targetR.healthComponent && targetR.healthComponent.body)
                {
                    if (BodyMeetsGrabConditions(targetR.healthComponent.body))
                    {
                        this.RgrabController = targetR.healthComponent.body.gameObject.AddComponent<SettGrabController>();
                        this.RgrabController.pivotTransform = this.FindModelChild("R_Hand");
                        base.characterMotor.disableAirControlUntilCollision = false;
                    }

                    if (NetworkServer.active)
                    {
                        //base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.startUp)
            {
                if (!this.hasFired)
                {
                    this.hasFired = true;
                    if (base.isAuthority)
                    {
                        Util.PlaySound("SettEVO", base.gameObject);

                        base.characterMotor.disableAirControlUntilCollision = true;

                        this.AttemptGrab(15f);

                        if (this.LgrabController || this.RgrabController)
                        {
                            string clip = "";
                            if (this.LgrabController && this.RgrabController)
                            {
                                clip = "Facebreaker_Both";
                            }
                            else if (this.RgrabController)
                            {
                                clip = "Facebreaker_Back";
                            }
                            else if (this.LgrabController)
                            {
                                clip = "Facebreaker_Front";
                            }

                            base.PlayAnimation("Fullbody, Override", clip, "FaceBreaker.playbackRate", this.duration);

                        }
                        if (!this.LgrabController && !this.RgrabController)
                        {
                            base.PlayAnimation("Fullbody, Override", "Facebreaker_Miss", "FaceBreaker.playbackRate", this.duration);
                        }
                    }
                }
            }

            if (this.stopwatch >= this.duration && base.isAuthority && this.hasFired)
            {
                if (this.LgrabController || this.RgrabController)
                {
                    Util.PlaySound("Hit", base.gameObject);

                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.radius = Facebreaker2.slamRadius;
                    blastAttack.procCoefficient = Facebreaker2.slamProcCoefficient;
                    blastAttack.position = base.characterBody.corePosition;
                    blastAttack.attacker = base.gameObject;
                    blastAttack.crit = base.RollCrit();
                    blastAttack.baseDamage = base.characterBody.damage * Facebreaker2.slamDamageCoefficient;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                    blastAttack.baseForce = Facebreaker2.slamForce;
                    blastAttack.teamIndex = TeamComponent.GetObjectTeam(blastAttack.attacker);
                    blastAttack.damageType = DamageType.Stun1s;
                    blastAttack.attackerFiltering = AttackerFiltering.NeverHit;
                    blastAttack.Fire();
                    base.characterMotor.velocity *= 0.1f;
                }
                if (this.LgrabController) this.LgrabController.Release();
                if (this.RgrabController) this.RgrabController.Release();
                this.outer.SetNextStateToMain();
                return;
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