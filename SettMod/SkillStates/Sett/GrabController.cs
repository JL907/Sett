using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class SettGrabController : MonoBehaviour
    {
        public Transform pivotTransform;
        public GameObject grabber;

        private EntityStateMachine grabberBodyMachine;
        private bool seenGrabberInShowStopper;
        private float age;
        private bool released;

        public static SettGrabController Apply(GameObject grabber, GameObject target)
        {
            if (!grabber || !target) return null;

            Transform pivot = FindHand(grabber);
            if (!pivot) return null;

            SettGrabController controller = target.GetComponent<SettGrabController>();
            if (!controller) controller = target.AddComponent<SettGrabController>();
            controller.pivotTransform = pivot;
            controller.grabber = grabber;
            controller.grabberBodyMachine = EntityStateMachine.FindByCustomName(grabber, "Body");

            if (NetworkServer.active)
            {
                CharacterBody grabberBody = grabber.GetComponent<CharacterBody>();
                if (grabberBody && !grabberBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
                {
                    grabberBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
            }

            return controller;
        }

        private static Transform FindHand(GameObject grabber)
        {
            ModelLocator locator = grabber.GetComponent<ModelLocator>();
            if (!locator || !locator.modelTransform) return null;
            ChildLocator childLocator = locator.modelTransform.GetComponent<ChildLocator>();
            return childLocator ? childLocator.FindChild("R_Hand") : null;
        }

        private CharacterBody body;
        private CapsuleCollider capsuleCollider;
        private CharacterDirection direction;
        private ModelLocator modelLocator;
        private Transform modelTransform;
        private CharacterMotor motor;
        private Quaternion originalRotation;
        private SphereCollider sphereCollider;

        public void Release()
        {
            if (this.released) return;
            this.released = true;
            if (this.modelLocator) this.modelLocator.enabled = true;
            if (this.modelTransform) this.modelTransform.rotation = this.originalRotation;
            if (this.direction) this.direction.enabled = true;
            if (this.capsuleCollider) this.capsuleCollider.enabled = true;
            if (this.sphereCollider) this.sphereCollider.enabled = true;
            Destroy(this);
        }

        private void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
            this.motor = this.GetComponent<CharacterMotor>();
            this.direction = this.GetComponent<CharacterDirection>();
            this.modelLocator = this.GetComponent<ModelLocator>();
            this.capsuleCollider = this.GetComponent<CapsuleCollider>();
            this.sphereCollider = this.GetComponent<SphereCollider>();

            if (this.direction) this.direction.enabled = false;

            if (this.capsuleCollider) this.capsuleCollider.enabled = false;

            if (this.sphereCollider) this.sphereCollider.enabled = false;

            if (this.modelLocator)
            {
                if (this.modelLocator.modelTransform)
                {
                    this.modelTransform = modelLocator.modelTransform;
                    this.originalRotation = this.modelTransform.rotation;

                    if (this.modelLocator.gameObject.name == "GreaterWispBody(Clone)")
                    {
                        this.modelLocator.dontReleaseModelOnDeath = true;
                        this.modelLocator.dontDetatchFromParent = true;
                    }

                    this.modelLocator.enabled = false;
                }
            }
        }

        private void FixedUpdate()
        {
            this.age += Time.fixedDeltaTime;

            if (!this.grabber)
            {
                this.Release();
                return;
            }
            if (this.grabberBodyMachine)
            {
                bool inShowStopper = this.grabberBodyMachine.state is ShowStopper;
                if (inShowStopper) this.seenGrabberInShowStopper = true;
                if ((!inShowStopper && this.seenGrabberInShowStopper) || (!this.seenGrabberInShowStopper && this.age > 1f))
                {
                    this.Release();
                    return;
                }
            }

            if (!this.pivotTransform)
            {
                this.Release();
                return;
            }

            if (this.motor && this.body && this.body.hasEffectiveAuthority)
            {
                this.motor.disableAirControlUntilCollision = true;
                this.motor.velocity = Vector3.zero;
                this.motor.rootMotion = Vector3.zero;

                this.motor.Motor.SetPosition(this.pivotTransform.position, true);
            }

            this.transform.position = this.pivotTransform.position;

            if (this.modelTransform)
            {
                this.modelTransform.position = this.pivotTransform.position;
                this.modelTransform.rotation = this.pivotTransform.rotation;
            }
        }
    }
}