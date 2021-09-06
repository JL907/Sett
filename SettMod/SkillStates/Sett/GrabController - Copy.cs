using RoR2;
using UnityEngine;

namespace SettMod.SkillStates
{
    public class SettGrabController2 : MonoBehaviour
    {
        public Transform pivotTransform;
        public Transform parentTransform;
        public Rigidbody parentRigidBody;
        public bool disableRotation = false;

        private CharacterBody body;
        private Rigidbody rigidBody;
        private CharacterMotor motor;
        private Quaternion originalRotation;
#pragma warning disable CS0108 // 'SettGrabController2.transform' hides inherited member 'Component.transform'. Use the new keyword if hiding was intended.
        private Transform transform;
#pragma warning restore CS0108 // 'SettGrabController2.transform' hides inherited member 'Component.transform'. Use the new keyword if hiding was intended.
        private ModelLocator modelLocator;
        private CharacterDirection direction;

        private void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
            this.rigidBody = this.GetComponent<Rigidbody>();
            this.transform = this.GetComponent<Transform>();
            this.motor = this.GetComponent<CharacterMotor>();
            this.direction = this.GetComponent<CharacterDirection>();
            this.modelLocator = this.GetComponent<ModelLocator>();

            if (this.direction) this.direction.enabled = false;
            if (this.modelLocator && this.modelLocator.transform)
            {
                this.transform = this.modelLocator.modelTransform;
                this.originalRotation = this.transform.rotation;
                this.modelLocator.enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (this.rigidBody && this.motor && this.pivotTransform)
            {
                this.motor.disableAirControlUntilCollision = true;
                this.motor.velocity = Vector3.zero;
                this.motor.rootMotion = Vector3.zero;

                this.motor.Motor.SetPositionAndRotation(this.pivotTransform.position, this.pivotTransform.rotation, true);

                this.rigidBody.MovePosition(this.pivotTransform.position);
                if (!this.disableRotation) this.rigidBody.MoveRotation(this.pivotTransform.rotation);
            }

            if (this.transform)
            {
                this.transform.position = this.pivotTransform.position;
                if (!this.disableRotation) this.transform.rotation = this.pivotTransform.rotation;
            }
            else
            {
                this.Release();
            }
        }

        public void Release()
        {
            if (this.modelLocator) this.modelLocator.enabled = true;
            if (this.direction) this.direction.enabled = true;
            Vector3 newParentPosition = new Vector3(this.transform.position.x, this.transform.position.y + 5, this.transform.position.z);
            if (this.transform)
            {
                this.transform.rotation = this.originalRotation;
                this.transform.position = newParentPosition;
            }
            if (this.parentRigidBody) this.parentRigidBody.MovePosition(newParentPosition);
            if (this.parentTransform) this.parentTransform.position = newParentPosition;


            Destroy(this);
        }
    }
}