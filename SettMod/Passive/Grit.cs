using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.Modules
{
    public class SyncAddGrit : INetMessage
    {
        private NetworkInstanceId bodyId;
        private float amount;

        public SyncAddGrit() { }

        public SyncAddGrit(NetworkInstanceId bodyId, float amount)
        {
            this.bodyId = bodyId;
            this.amount = amount;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.bodyId);
            writer.Write(this.amount);
        }

        public void Deserialize(NetworkReader reader)
        {
            this.bodyId = reader.ReadNetworkId();
            this.amount = reader.ReadSingle();
        }

        public void OnReceived()
        {
            if (!NetworkServer.active) return;
            GameObject bodyObject = Util.FindNetworkObject(this.bodyId);
            GritComponent grit = bodyObject ? bodyObject.GetComponent<GritComponent>() : null;
            if (grit) grit.AddGrit(this.amount);
        }
    }

    public class GritComponent : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        private CharacterBody body;
        private EntityStateMachine outer = null;
        public const float GritMaxUptime = 4.0f;
        public float gritUptimeStopwatch = 0.0f;
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

        private bool coreAssigned;
        private Transform core;
        private float effectCheckTimer;
        private TemporaryVisualEffect barrierEffect;

        private void AssignCore()
        {
            this.coreAssigned = true;
            if (!this.body) return;

            ModelLocator modelLocator = this.body.modelLocator;
            Transform modelBase = modelLocator && modelLocator.modelBaseTransform ? modelLocator.modelBaseTransform : this.transform.Find("ModelBase");
            if (!modelBase) return;

            Transform core = modelBase.Find("Core");
            if (!core)
            {
                core = new GameObject("Core").transform;
                core.SetParent(modelBase, false);
                core.localPosition = new Vector3(0f, 1.5f, 0f);
            }
            this.core = core;
            this.body.coreTransform = core;
        }

        private void RetargetVisualEffects()
        {
            if (!this.core || !this.body || !this.body.healthComponent) return;

            foreach (TemporaryVisualEffect effect in UnityEngine.Object.FindObjectsOfType<TemporaryVisualEffect>())
            {
                if (effect.healthComponent != this.body.healthComponent) continue;

                if (effect.parentTransform != this.core)
                {
                    effect.parentTransform = this.core;
                    effect.transform.position = this.core.position;
                }

                if (effect.name.Contains("Barrier") && effect != this.barrierEffect && effect.visualState != TemporaryVisualEffect.VisualState.Exit)
                {
                    this.barrierEffect = effect;
                }
            }
        }

        private void LateUpdate()
        {
            if (!this.body || !this.body.healthComponent || this.body.healthComponent.barrier <= 0f) return;

            if (!this.barrierEffect || this.barrierEffect.visualState == TemporaryVisualEffect.VisualState.Exit) this.RetargetVisualEffects();
            if (!this.barrierEffect) return;

            if (this.core && this.barrierEffect.parentTransform != this.core)
            {
                this.barrierEffect.parentTransform = this.core;
                this.barrierEffect.transform.position = this.core.position;
            }
        }

        private void FixedUpdate()
        {
            if (!this.coreAssigned) this.AssignCore();

            if (this.body && this.body.healthComponent && this.body.healthComponent.barrier > 0f)
            {
                this.effectCheckTimer -= Time.fixedDeltaTime;
                if (this.effectCheckTimer <= 0f)
                {
                    this.effectCheckTimer = 0.2f;
                    this.RetargetVisualEffects();
                }
            }
            else
            {
                this.effectCheckTimer = 0f;
            }
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
        }

        public void AddGritAuthority(float value)
        {
            if (NetworkServer.active)
            {
                this.AddGrit(value);
                return;
            }
            NetworkIdentity identity = base.GetComponent<NetworkIdentity>();
            if (identity) new SyncAddGrit(identity.netId, value).Send(NetworkDestination.Server);
        }

        public void AddGrit(float amount)
        {
            if (!NetworkServer.active) return;
            float maxGrit = this.GetMaxGrit();
            this.NetworkGrit = Mathf.Min(Mathf.Max(0, this.NetworkGrit + amount), maxGrit);
        }

        public float GetMaxGrit()
        {
            return this.GetMaxHealth() / 2f;
        }

        public float GetCurrentGrit()
        {
            return Mathf.Max(0, this.NetworkGrit);
        }

        public float GetMaxHealth()
        {
            if (this.body.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0)
            {
                return this.body.maxShield;
            }
            return this.body.maxHealth;
        }

        public float GetCurrentHealth()
        {
            if (this.body.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0)
            {
                return this.body.healthComponent.shield;
            }
            return this.body.healthComponent.health;
        }

        public float GetMissingHealth()
        {
            return this.GetMaxHealth() - GetCurrentHealth();
        }

        public float GetSettRegen()
        {
            return ((this.GetMissingHealth() / this.GetMaxHealth()) / 0.05f);
        }

        public void BuffRegen()
        {
            int missingHealthPer5 = (int)Mathf.Round(this.GetSettRegen());
            if (NetworkServer.active)
            {
                int buffCount = this.body.GetBuffCount(Modules.Buffs.regenBuff);

                if (buffCount < missingHealthPer5)
                {
                    this.body.AddBuff(Modules.Buffs.regenBuff);
                }
                if (buffCount > missingHealthPer5)
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
            else
            {
                float decayAmount = 0.3f;
                float finalGritValue = Mathf.Max(this.NetworkGrit - this.NetworkGrit * decayAmount, 0);
                float decayDuration = 1.0f;
                this.NetworkGrit = Mathf.Lerp(this.NetworkGrit, finalGritValue, Time.fixedDeltaTime / decayDuration);
                if (this.NetworkGrit < 0.01f)
                {
                    this.NetworkGrit = 0;
                    gritUptimeStopwatch = 0;
                }
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