using RoR2;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class SettGrabController : MonoBehaviour
    {
        public Transform pivotTransform;

        private CharacterBody body;
        private CharacterDirection direction;
        public ModelLocator modelLocator;
        private Transform modelTransform;
        private CharacterMotor motor;
        private CapsuleCollider capsuleCollider;
        private SphereCollider sphereCollider;
        private Quaternion originalRotation;

        public void Release()
        {
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
            if (this.motor)
            {
                this.motor.disableAirControlUntilCollision = true;
                this.motor.velocity = Vector3.zero;
                this.motor.rootMotion = Vector3.zero;

                this.motor.Motor.SetPosition(this.pivotTransform.position, true);
            }

            if (this.pivotTransform)
            {
                this.transform.position = this.pivotTransform.position;
            }
            else
            {
                this.Release();
            }

            if (this.modelTransform)
            {
                this.modelTransform.position = this.pivotTransform.position;
                this.modelTransform.rotation = this.pivotTransform.rotation;
            }
        }
    }
}