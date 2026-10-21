using BepInEx.Configuration;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace Runes
{
    internal sealed class Electrocute : Keystone
    {
        internal static ConfigEntry<float> window;
        internal static ConfigEntry<float> cooldown;
        internal static ConfigEntry<float> damage;
        internal static ConfigEntry<float> damagePerLevelStep;

        internal static BuffDef debuff;

        public override KeystoneType Type => KeystoneType.Electrocute;
        public override string Id => "ELECTROCUTE";
        public override string DisplayName => "Electrocute";
        public override string IconName => "Electrocute_rune";

        internal override void BindConfig(ConfigFile config)
        {
            const string section = "Electrocute";
            window = config.Bind(section, "Stack Window", 3f, "Seconds each Electrocute stack lasts on a target.");
            cooldown = config.Bind(section, "Cooldown Per Target", 5f, "");
            damage = config.Bind(section, "Damage Coefficient", 6f, "Lightning damage as a multiple of your damage stat.");
            damagePerLevelStep = config.Bind(section, "Damage Coefficient Per 4 Levels", 0.75f, "");
        }

        internal override void CreateBuffs()
        {
            debuff = KeystoneContent.AddBuff("KeystoneElectrocuteDebuff", icon, true, true);
        }

        internal override string Description()
        {
            return Hit + " apply <color=#c9aa71>1</color> stack to enemies. Applying <color=#ffffff>3</color> stacks to a target within " +
                Num(window.Value) + " seconds strikes it with lightning for <color=#f68835>" + Pct(damage.Value) + "</color> " +
                "<color=#d62d20>(+" + Pct(damagePerLevelStep.Value) + " every 4 levels)</color> damage. " +
                Num(cooldown.Value) + " second cooldown per target.";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            ElectrocuteHandler electrocute = damageReport.victimBody.GetComponent<ElectrocuteHandler>();
            if (!electrocute)
            {
                electrocute = damageReport.victimBody.gameObject.AddComponent<ElectrocuteHandler>();
                electrocute.attackerBody = handler.body;
            }
            electrocute.AddStack();
        }
    }

    /// <summary>
    /// Added to an enemy (server side) the first time it's hit by someone running Electrocute.
    /// Three stacks fire a lightning strike, then the target is immune until the component expires.
    /// </summary>
    public class ElectrocuteHandler : MonoBehaviour
    {
        private const int MaxStacks = 3;

        public CharacterBody attackerBody;

        private CharacterBody body;
        private float stopwatch;
        private bool fired;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
        }

        public void AddStack()
        {
            if (fired || !body) return;
            if (body.GetBuffCount(Electrocute.debuff) < MaxStacks)
            {
                body.AddTimedBuff(Electrocute.debuff, Electrocute.window.Value);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!fired && body && body.GetBuffCount(Electrocute.debuff) >= MaxStacks)
            {
                Fire();
            }

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= Electrocute.cooldown.Value)
            {
                Destroy(this);
            }
        }

        private void Fire()
        {
            fired = true;
            body.SetBuffCount(Electrocute.debuff.buffIndex, 0);

            if (!attackerBody || !body.mainHurtBox) return;

            float levelSteps = Mathf.Floor(attackerBody.level / 4f);
            float coefficient = Electrocute.damage.Value + levelSteps * Electrocute.damagePerLevelStep.Value;

            OrbManager.instance.AddOrb(new SimpleLightningStrikeOrb
            {
                attacker = attackerBody.gameObject,
                damageColorIndex = DamageColorIndex.Item,
                damageValue = attackerBody.damage * coefficient,
                isCrit = Util.CheckRoll(attackerBody.crit, attackerBody.master),
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f,
                target = body.mainHurtBox
            });
        }
    }
}
