using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SettMod.SkillStates.Keystone
{
    public class KeyStoneHandler : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        public const float conquererUpTime = 4.0f;

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
            if (this.keyStone.skillNameToken == "JojoSETT_CONQUERER_NAME" && this.body.GetBuffCount(Modules.Buffs.conquererBuff) < 12)
            {
                this.body.AddBuff(Modules.Buffs.conquererBuff);
            }
            if (this.body.GetBuffCount(Modules.Buffs.conquererBuff) >= 12)
            {
                float damageHeal = damageReport.damageDealt * 0.03f;
                this.healthComponent.Heal(damageHeal, default(ProcChainMask), true);
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
            if (this.keyStone.skillNameToken == "JojoSETT_CONQUERER_NAME")
            {
                if (UptimeStopwatch < conquererUpTime)
                {
                    UptimeStopwatch += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime < 1f)
                {
                    throttleUpdateTime += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime >= 1f && UptimeStopwatch > conquererUpTime && this.body.GetBuffCount(Modules.Buffs.conquererBuff) > 0)
                {
                    this.body.RemoveBuff(Modules.Buffs.conquererBuff);
                    throttleUpdateTime = 0f;
                }
                if (this.body.GetBuffCount(Modules.Buffs.conquererBuff) <= 0)
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

                if (throttleUpdateTime < 1f)
                {
                    throttleUpdateTime += Time.fixedDeltaTime;
                }

                if (throttleUpdateTime >= 1f && UptimeStopwatch > lethalUpTime && this.body.GetBuffCount(Modules.Buffs.lethalBuff) > 0)
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