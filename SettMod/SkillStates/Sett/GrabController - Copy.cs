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
        private CharacterMotor motor;
        private Quaternion originalRotation;
        private Transform modelTransform;
        private ModelLocator modelLocator;
        private CharacterDirection direction;

        private void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
            this.modelTransform = this.GetComponent<Transform>();
            this.motor = this.GetComponent<CharacterMotor>();
            this.direction = this.GetComponent<CharacterDirection>();
            this.modelLocator = this.GetComponent<ModelLocator>();

            if (this.direction) this.direction.enabled = false;

            if (this.modelLocator)
            {
                this.modelTransform = this.modelLocator.modelTransform;
                this.originalRotation = this.transform.rotation;
                this.modelLocator.enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (this.motor)
            {
                this.motor.disableAirControlUntilCollision = true;
                this.motor.velocity = Vector3.zero;
                this.motor.rootMotion = Vector3.zero;

                this.motor.Motor.SetPosition(this.pivotTransform.position, false);
                if (!this.disableRotation) this.motor.Motor.SetRotation(this.pivotTransform.rotation, false);
 
            }

            if (this.pivotTransform)
            {
                this.transform.position = this.pivotTransform.position;
                if (!this.disableRotation) this.transform.rotation = this.pivotTransform.rotation;
            }
            else
            {
                this.Release();
            }

            if (this.modelTransform)
            {
                this.modelTransform.position = this.pivotTransform.position;
                if (!this.disableRotation) this.modelTransform.rotation = this.pivotTransform.rotation;
            }
        }

        public void Release()
        {
            if (this.modelLocator) this.modelLocator.enabled = true;
            if (this.modelTransform) this.modelTransform.rotation = this.originalRotation;
            if (this.direction) this.direction.enabled = true;
            Vector3 newParentPosition = new Vector3(this.modelTransform.position.x, this.modelTransform.position.y + 5, this.modelTransform.position.z);
            if (this.parentRigidBody) this.parentRigidBody.MovePosition(newParentPosition);
            if (this.parentTransform) this.parentTransform.position = newParentPosition;


            Destroy(this);
        }
    }
}