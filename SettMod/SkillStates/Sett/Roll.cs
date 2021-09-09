using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class Roll2 : BaseSkillState
    {
        public static float duration = 0.4f;
        public static float maxSpeed = 35f;
        public static float minSpeed = 15f;
        public static string dodgeSoundString = "SettDash";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public float dashSpeed { get; private set; }
        protected Vector3 dashVelocity;

        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
            {
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.disableAirControlUntilCollision = false;
                this.dashVelocity = Roll2.CalculateLungeVelocity(base.characterMotor.velocity, base.GetAimRay().direction, Roll2.duration, Roll2.minSpeed, Roll2.maxSpeed);
                base.characterMotor.velocity = this.dashVelocity;
                base.characterDirection.forward = base.characterMotor.velocity.normalized;
                this.dashSpeed = base.characterMotor.velocity.magnitude;
            }
            base.PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", 0.7f);
            Util.PlaySound(Roll2.dodgeSoundString, base.gameObject);

        }

#pragma warning disable CS0108 // 'Roll2.GetAimRay()' hides inherited member 'BaseState.GetAimRay()'. Use the new keyword if hiding was intended.
        protected Ray GetAimRay()
#pragma warning restore CS0108 // 'Roll2.GetAimRay()' hides inherited member 'BaseState.GetAimRay()'. Use the new keyword if hiding was intended.
        {
            if (base.inputBank)
            {
                return new Ray(base.inputBank.aimOrigin, base.inputBank.aimDirection);
            }
            return new Ray(base.transform.position, base.transform.forward);
        }

        public static Vector3 CalculateLungeVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLungeSpeed, float maxLungeSpeed)
        {
            currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
            return currentVelocity + aimDirection * Mathf.Lerp(minLungeSpeed, maxLungeSpeed, charge);
        }

        private void AttemptSlam()
        {
            Ray aimRay = base.GetAimRay();

            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(base.GetTeam()),
                filterByLoS = true,
                searchOrigin = base.transform.position,
                searchDirection = Random.onUnitSphere,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = 8f,
                maxAngleFilter = 360f
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);

            HurtBox target = search.GetResults().FirstOrDefault<HurtBox>();
            if (target)
            {
                if (target.healthComponent && target.healthComponent.body && base.isAuthority)
                {
                    this.outer.SetNextState(new ShowStopper
                    {

                    });
                }
            }
        }



        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Roll2.dodgeFOV, 60f, base.fixedAge / Roll2.duration);

            if (base.isAuthority)
            {
                base.characterMotor.velocity = this.dashVelocity;
                base.characterDirection.forward = this.dashVelocity;
                base.characterBody.isSprinting = true;
            }

            this.AttemptSlam();

            if (base.fixedAge >= Roll2.duration && isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}