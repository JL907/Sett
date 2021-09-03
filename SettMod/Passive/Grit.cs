using UnityEngine;
using EntityStates;
using RoR2;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace SettMod.Modules
{
    public class GritComponent : NetworkBehaviour , IOnDamageDealtServerReceiver , IOnTakeDamageServerReceiver
    {
        private CharacterBody body;
        private EntityStateMachine outer = null;
        public const float GritUpdateTime = 1.0f;
        public const float GritMaxUptime = 4.0f;
        public const float MaxTrottleUpdateTime = 1.0f;
        public float gritUptimeStopwatch = 0.0f;
        private float throttleUpdateTime = 0.0f;
        public float NetworkGrit
        {
            get
            {
                return this.grit;
            }
            [param: In]
            set
            {
                base.SetSyncVar<float>(value, ref this.grit, 1U);
            }
        }


        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.outer = base.GetComponent<EntityStateMachine>();
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                this.ServerFixedUpdate();
            }
            if (isAuthority)
            {
                this.AuthorityFixedUpdate();
            }
        }

        private void AuthorityFixedUpdate()
        {
            throw new NotImplementedException();
        }

        public void AddGritAuthority(float value)
        {
            if (NetworkServer.active)
            {
                this.AddGrit(value);
                return;
            }
            this.CmdAddGrit(value);
        }

        [Command]
        public void CmdAddGrit(float value)
        {
            this.AddGrit(value);
        }

        [Server]
        public void AddGrit(float amount)
        {
            float maxGrit = this.GetMaxGrit();
            this.NetworkGrit = Mathf.Min(Mathf.Max(0, this.NetworkGrit + amount), maxGrit);
        }

        public float GetMaxGrit()
        {
            return this.body.maxHealth / 2f;
        }

        public float GetCurrentGrit()
        {
            return Mathf.Max(0, this.NetworkGrit);
        }


        private void ServerFixedUpdate()
        {
            if(gritUptimeStopwatch < GritMaxUptime)
            {
                gritUptimeStopwatch += Time.fixedDeltaTime;
            }
            if(throttleUpdateTime < MaxTrottleUpdateTime)
            {
                throttleUpdateTime += Time.fixedDeltaTime;
            }
            if (throttleUpdateTime >= MaxTrottleUpdateTime)
            {
                throttleUpdateTime = 0f;
                var gritDecayAmount = 0.3f;
                var snapShotGrit = this.NetworkGrit;
                this.NetworkGrit -= Mathf.Max(snapShotGrit * gritDecayAmount, 0);
            }
            if (this.NetworkGrit <= 0f)
            {
                gritUptimeStopwatch = 0f;
                this.NetworkGrit = 0f;
            }
        }

        protected bool isAuthority
        {
            get
            {
                return Util.HasEffectiveAuthority(this.outer.networkIdentity);
            }
        }


        public void OnDamageDealtServer(DamageReport damageReport)
        {
            //throw new NotImplementedException();
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            AddGritAuthority(damageReport.damageInfo.damage);
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(this.grit);
                return true;
            }
            bool flag = false;
            if ((base.syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(base.syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(this.grit);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(base.syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                this.grit = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                this.grit = reader.ReadSingle();
            }
        }


        [HideInInspector]
        [SyncVar]
        [Tooltip("How much Grit this object has.")]
        public float grit = 0f;
    }

    
}