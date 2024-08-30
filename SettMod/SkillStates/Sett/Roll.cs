using EntityStates;
using RoR2;
using System.Linq;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class Dash : BaseSkillState
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

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.disableAirControlUntilCollision = false;
            this.dashVelocity = Dash.CalculateLungeVelocity(base.characterMotor.velocity, this.GetAimRay().direction, Dash.duration, Dash.minSpeed, Dash.maxSpeed);
            base.characterMotor.velocity = this.dashVelocity;
            base.characterDirection.forward = base.characterMotor.velocity.normalized;
            this.dashSpeed = base.characterMotor.velocity.magnitude;

            base.PlayCrossfade("FullBody, Override", "Roll", "Roll.playbackRate", 0.7f, 0.05f);
            Util.PlaySound(Dash.dodgeSoundString, base.gameObject);
        }

        public static Vector3 CalculateLungeVelocity(Vector3 currentVelocity, Vector3 aimDirection, float charge, float minLungeSpeed, float maxLungeSpeed)
        {
            currentVelocity = ((Vector3.Dot(currentVelocity, aimDirection) < 0f) ? Vector3.zero : Vector3.Project(currentVelocity, aimDirection));
            return currentVelocity + aimDirection * Mathf.Lerp(minLungeSpeed, maxLungeSpeed, charge);
        }

        private void AttemptSlam()
        {
            Ray aimRay = this.GetAimRay();

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
                if (target.healthComponent && target.healthComponent.body)
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

            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Dash.dodgeFOV, 60f, base.fixedAge / Dash.duration);

            base.characterMotor.velocity = this.dashVelocity;
            base.characterDirection.forward = this.dashVelocity;
            base.characterBody.isSprinting = true;

            this.AttemptSlam();

            if (base.fixedAge >= Dash.duration && isAuthority)
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
            return InterruptPriority.Skill;
        }
    }
}