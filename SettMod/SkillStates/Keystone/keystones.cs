using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SettMod.SkillStates.Keystone
{
    public class KeyStoneHandler : NetworkBehaviour, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
    {
        public float conquerorMaxStacks = 12f;

        [FormerlySerializedAs("Keystone")]
        public GenericSkill keyStone;

        public KeyStones keyStoneType = KeyStones.None;
        public float lethalMaxStacks = 6f;
        public float UptimeStopwatch = 0.0f;
        private CharacterBody body;
        private float conquerorUpTime = 4.0f;
        private HealthComponent healthComponent;
        private float lethalUpTime = 6.0f;
        private float MaxthrottleTime = 0.5f;
        private EntityStateMachine outer = null;
        private float throttleUpdateTime = 0.0f;

        public enum KeyStones
        {
            None = 0,
            Conqueror = 1,
            Lethal = 2,
            PhaseRush = 4,
            Electrocute = 8
        }

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
            if (damageReport is null) return;

            if (damageReport.victimBody && damageReport.attacker && damageReport.attackerBody.baseNameToken == "SETT_NAME" && DamageAPI.HasModdedDamageType(damageReport.damageInfo, SettPlugin.settDamage))
            {
                UptimeStopwatch = 0f;

                if (this.keyStoneType is KeyStones.PhaseRush)
                {
                    if (damageReport.victimBody.GetBuffCount(Modules.Buffs.phaseRushDebuff) < 3)
                    {
                        damageReport.victimBody.AddTimedBuff(Modules.Buffs.phaseRushDebuff, 4);
                    }

                    if (damageReport.victimBody.GetBuffCount(Modules.Buffs.phaseRushDebuff) >= 3 && this.body.GetBuffCount(Modules.Buffs.movementSpeedBuff) < 1)
                    {
                        this.body.AddTimedBuff(Modules.Buffs.movementSpeedBuff, 3);
                    }
                }

                if (this.keyStoneType is KeyStones.Electrocute)
                {
                    ElectrocuteHandler electrocuteHandler = damageReport.victimBody.gameObject.GetComponent<ElectrocuteHandler>();
                    if (!electrocuteHandler)
                        damageReport.victimBody.gameObject.AddComponent<ElectrocuteHandler>().attackerBody = damageReport.attackerBody;
                    else
                    {
                        electrocuteHandler.AddStack();
                    }
                }

                AddKeyStoneBuff();
                if (this.keyStoneType is KeyStones.Conqueror)
                {
                    if (GetKeyStoneBuffCount() >= GetKeyStoneMaxStacks())
                    {
                        float damageHeal = damageReport.damageDealt * 0.06f;
                        this.healthComponent.Heal(damageHeal, default, true);
                    }
                }
            }
        }

        public void OnTakeDamageServer(DamageReport damageReport)
        {
        }

        private void AddKeyStoneBuff()
        {
            if (GetKeyStoneBuffCount() < GetKeyStoneMaxStacks())
            {
                if (this.keyStoneType is KeyStones.Conqueror) this.body.AddBuff(Modules.Buffs.conquerorBuff);
                if (this.keyStoneType is KeyStones.Lethal) this.body.AddBuff(Modules.Buffs.lethalBuff);
            }
        }

        private void AuthorityFixedUpdate()
        {
            //throw new NotImplementedException();
        }

        private void CheckKeyStone()
        {
            if (this.keyStone.skillNameToken == "SETT_CONQUEROR_NAME") this.keyStoneType = KeyStones.Conqueror;
            if (this.keyStone.skillNameToken == "SETT_LETHAL_NAME") this.keyStoneType = KeyStones.Lethal;
            if (this.keyStone.skillNameToken == "SETT_PHASE_RUSH_NAME") this.keyStoneType = KeyStones.PhaseRush;
            if (this.keyStone.skillNameToken == "SETT_ELECTROCUTE_NAME") this.keyStoneType = KeyStones.Electrocute;
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

        private void Update()
        {
            CheckKeyStone();
        }
    }
}