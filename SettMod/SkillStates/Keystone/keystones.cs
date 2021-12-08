using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SettMod.SkillStates.Keystone
{
    public class KeyStoneHandler : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        [FormerlySerializedAs("Keystone")]
        public GenericSkill keyStone;

        public float UptimeStopwatch = 0.0f;
        private CharacterBody body;
        private HealthComponent healthComponent;
        private EntityStateMachine outer = null;
        public float conquerorMaxStacks = 12f;
        public float lethalMaxStacks = 6f;
        private float throttleUpdateTime = 0.0f;
        private float MaxthrottleTime = 0.5f;
        private float conquerorUpTime = 4.0f;
        private float lethalUpTime = 6.0f;
        protected bool isAuthority
        {
            get
            {
                return Util.HasEffectiveAuthority(this.outer.networkIdentity);
            }
        }
        public enum KeyStones
        {
            None = 0,
            Conqueror = 1,
            Lethal = 2
        }

        public KeyStones keyStoneType = KeyStones.None;

        public void Awake()
        {
            this.body = base.GetComponent<CharacterBody>();
            this.outer = base.GetComponent<EntityStateMachine>();
            this.healthComponent = base.GetComponent<HealthComponent>();
        }

        private void CheckKeyStone()
        {
            if (this.keyStone.skillNameToken == "JojoSETT_CONQUEROR_NAME") this.keyStoneType = KeyStones.Conqueror;
            if (this.keyStone.skillNameToken == "JojoSETT_LETHAL_NAME") this.keyStoneType = KeyStones.Lethal;
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            if(damageReport.attackerBody == this.body)
            {
                UptimeStopwatch = 0f;
                AddKeyStoneBuff();
                if (this.keyStoneType is KeyStones.Conqueror)
                {
                    if (GetKeyStoneBuffCount() >= GetKeyStoneMaxStacks())
                    {
                        float damageHeal = damageReport.damageDealt * 0.03f;
                        this.healthComponent.Heal(damageHeal, default, true);
                    }
                }
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

        private void Update()
        {
            CheckKeyStone();
        }

        private float GetKeyStoneBuffCount()
        {
            if (this.keyStoneType is KeyStones.Conqueror) return this.body.GetBuffCount(Modules.Buffs.conquerorBuff);
            if (this.keyStoneType is KeyStones.Lethal) return this.body.GetBuffCount(Modules.Buffs.lethalBuff);
            return 0;
        }

        private float GetKeyStoneMaxStacks()
        {
            if (this.keyStoneType is KeyStones.Conqueror) return this.conquerorMaxStacks;
            if (this.keyStoneType is KeyStones.Lethal) return this.lethalMaxStacks;
            return 6;
        }

        private float GetKeyStoneUpTime()
        {
            if (this.keyStoneType is KeyStones.Conqueror) return this.conquerorUpTime;
            if (this.keyStoneType is KeyStones.Lethal) return this.lethalUpTime;
            return 5;
        }

        private void RemoveKeyStoneBuff()
        {
            if (this.keyStoneType is KeyStones.Conqueror) this.body.RemoveBuff(Modules.Buffs.conquerorBuff);
            if (this.keyStoneType is KeyStones.Lethal) this.body.RemoveBuff(Modules.Buffs.lethalBuff);
        }

        private void AddKeyStoneBuff()
        {
            if (GetKeyStoneBuffCount() < GetKeyStoneMaxStacks())
            {
                if (this.keyStoneType is KeyStones.Conqueror) this.body.AddBuff(Modules.Buffs.conquerorBuff);
                if (this.keyStoneType is KeyStones.Lethal) this.body.AddBuff(Modules.Buffs.lethalBuff);
            }

        }

        private void ServerFixedUpdate()
        {
            if (UptimeStopwatch < GetKeyStoneUpTime())
            {
                UptimeStopwatch += Time.fixedDeltaTime;
            }

            if (throttleUpdateTime < MaxthrottleTime)
            {
                throttleUpdateTime += Time.fixedDeltaTime;
            }

            if (throttleUpdateTime >= MaxthrottleTime && UptimeStopwatch > GetKeyStoneUpTime() && GetKeyStoneBuffCount() > 0)
            {
                RemoveKeyStoneBuff();
                throttleUpdateTime = 0f;
            }
            if (GetKeyStoneBuffCount() <= 0)
            {
                UptimeStopwatch = 0f;
            }
        }
    }
}