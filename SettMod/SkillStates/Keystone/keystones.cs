using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SettMod.SkillStates.Keystone
{
    public class KeyStoneHandler : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        public const float conquerorUpTime = 4.0f;

        public const float lethalUpTime = 6.0f;

        [FormerlySerializedAs("Keystone")]
        public GenericSkill keyStone;

        public float UptimeStopwatch = 0.0f;
        private CharacterBody body;
        private HealthComponent healthComponent;
        private EntityStateMachine outer = null;
        private float throttleUpdateTime = 0.0f;

        protected bool isAuthority
        {
            get
            {
                return Util.HasEffectiveAuthority(this.outer.networkIdentity);
            }
        }

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.outer = base.GetComponent<EntityStateMachine>();
            this.healthComponent = base.GetComponent<HealthComponent>();
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            UptimeStopwatch = 0f;
            if (this.keyStone.skillNameToken == "JojoSETT_CONQUEROR_NAME" && this.body.GetBuffCount(Modules.Buffs.conquerorBuff) < 12)
            {
                this.body.AddBuff(Modules.Buffs.conquerorBuff);
            }
            if (this.body.GetBuffCount(Modules.Buffs.conquerorBuff) >= 12)
            {
                float damageHeal = damageReport.damageDealt * 0.06f;
                this.healthComponent.Heal(damageHeal, default, true);
            }
            if (this.keyStone.skillNameToken == "JojoSETT_LETHAL_NAME" && this.body.GetBuffCount(Modules.Buffs.lethalBuff) < 6)
            {
                this.body.AddBuff(Modules.Buffs.lethalBuff);
            }
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
        }

        private void AuthorityFixedUpdate()
        {
            //throw new NotImplementedException();
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

        private void ServerFixedUpdate()
        {
            if (this.keyStone.skillNameToken == "JojoSETT_CONQUEROR_NAME")
            {
                if (UptimeStopwatch < conquerorUpTime)
                {
                    UptimeStopwatch += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime < 0.5f)
                {
                    throttleUpdateTime += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime >= 0.5f && UptimeStopwatch > conquerorUpTime && this.body.GetBuffCount(Modules.Buffs.conquerorBuff) > 0)
                {
                    this.body.RemoveBuff(Modules.Buffs.conquerorBuff);
                    throttleUpdateTime = 0f;
                }
                if (this.body.GetBuffCount(Modules.Buffs.conquerorBuff) <= 0)
                {
                    UptimeStopwatch = 0f;
                }
            }
            else if (this.keyStone.skillNameToken == "JojoSETT_LETHAL_NAME")
            {
                if (UptimeStopwatch < lethalUpTime)
                {
                    UptimeStopwatch += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime < 0.5f)
                {
                    throttleUpdateTime += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime >= 0.5f && UptimeStopwatch > lethalUpTime && this.body.GetBuffCount(Modules.Buffs.lethalBuff) > 0)
                {
                    this.body.RemoveBuff(Modules.Buffs.lethalBuff);
                    throttleUpdateTime = 0f;
                }
                if (this.body.GetBuffCount(Modules.Buffs.lethalBuff) <= 0)
                {
                    UptimeStopwatch = 0f;
                }
            }
        }
    }
}