using RoR2;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.Modules
{
    public class GritComponent : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        private CharacterBody body;
        private EntityStateMachine outer = null;
#pragma warning disable CS0169 // The field 'GritComponent.bodyInfo' is never used
        private BodyInfo bodyInfo;
#pragma warning restore CS0169 // The field 'GritComponent.bodyInfo' is never used
        public const float GritUpdateTime = 1.0f;
        public const float GritMaxUptime = 4.0f;
        public const float MaxTrottleUpdateTime = 1.0f;
        public float gritUptimeStopwatch = 0.0f;
        private float throttleUpdateTime = 0.0f;
#pragma warning disable CS0414 // The field 'GritComponent.isDecaying' is assigned but its value is never used
        private bool isDecaying = false;
#pragma warning restore CS0414 // The field 'GritComponent.isDecaying' is assigned but its value is never used
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
            //throw new NotImplementedException();
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

        public float GetMissingHealth()
        {
            return this.body.maxHealth - this.body.healthComponent.health;
        }

        public float GetSettRegen()
        {
            return ((this.GetMissingHealth() / this.body.maxHealth) / 0.05f);
        }

        public void BuffRegen()
        {
            int missingHealthPer10 = (int)Mathf.Round(this.GetSettRegen());
            if (NetworkServer.active)
            {
                int buffCount = this.body.GetBuffCount(Modules.Buffs.regenBuff);

                if (buffCount < missingHealthPer10)
                {
                    this.body.AddBuff(Modules.Buffs.regenBuff);
                }
                if (buffCount > missingHealthPer10)
                {
                    this.body.RemoveBuff(Modules.Buffs.regenBuff);
                }
            }
        }

        private void ServerFixedUpdate()
        {
            this.BuffRegen();
            if (gritUptimeStopwatch < GritMaxUptime)
            {
                gritUptimeStopwatch += Time.fixedDeltaTime;
            }
            if (throttleUpdateTime < MaxTrottleUpdateTime)
            {
                throttleUpdateTime += Time.fixedDeltaTime;
            }
            if (throttleUpdateTime >= MaxTrottleUpdateTime && gritUptimeStopwatch > GritMaxUptime)
            {

                throttleUpdateTime = 0f;
                var gritDecayAmount = 0.2f;
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
            gritUptimeStopwatch = 0f;
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