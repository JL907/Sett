using BepInEx.Configuration;
using RoR2;

namespace Runes
{
    internal sealed class Conqueror : Keystone
    {
        internal static ConfigEntry<int> maxStacks;
        internal static ConfigEntry<float> duration;
        internal static ConfigEntry<float> damagePerStack;
        internal static ConfigEntry<float> damagePerStackPerLevelStep;
        internal static ConfigEntry<float> healFraction;

        internal static BuffDef buff;

        public override KeystoneType Type => KeystoneType.Conqueror;
        public override string Id => "CONQUEROR";
        public override string DisplayName => "Conqueror";
        public override string IconName => "Conqueror_rune";

        internal override void BindConfig(ConfigFile config)
        {
            const string section = "Conqueror";
            maxStacks = config.Bind(section, "Max Stacks", 12, "");
            duration = config.Bind(section, "Stack Duration", 4f, "Seconds without hitting anything before stacks start falling off.");
            damagePerStack = config.Bind(section, "Base Damage Per Stack", 0.6f, "Flat base damage added per stack.");
            damagePerStackPerLevelStep = config.Bind(section, "Base Damage Per Stack Per 4 Levels", 0.045f, "");
            healFraction = config.Bind(section, "Heal Fraction At Max Stacks", 0.06f, "Fraction of damage dealt returned as healing while fully stacked.");
        }

        internal override void CreateBuffs()
        {
            // Buff names are prefixed so they don't collide with the old SettMod buffs if someone runs an outdated Sett.
            buff = KeystoneContent.AddBuff("KeystoneConquerorBuff", icon, true, false);
        }

        internal override string Description()
        {
            return Hit + " against enemies grant <color=#ffffff>1</color> stack of Conqueror, up to " + maxStacks.Value + " stacks. " +
                "Each stack grants <color=#f68835>" + Num(damagePerStack.Value) + "</color> " +
                "<color=#d62d20>(+" + Num(damagePerStackPerLevelStep.Value) + " every 4 levels)</color> bonus base damage. " +
                "While fully stacked, <color=#c9aa71>heal</color> for <color=#008744>" + Pct(healFraction.Value) + " of damage dealt</color>.";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            handler.AddStack(buff, maxStacks.Value);
            if (handler.body.GetBuffCount(buff) >= maxStacks.Value && handler.healthComponent)
            {
                handler.healthComponent.Heal(damageReport.damageDealt * healFraction.Value, default(ProcChainMask), true);
            }
        }

        internal override void FixedUpdateServer(KeystoneHandler handler)
        {
            handler.DecayStacks(buff, duration.Value);
        }

        internal override void RecalculateStats(CharacterBody body, float levelSteps)
        {
            int stacks = body.GetBuffCount(buff);
            if (stacks > 0)
            {
                body.damage += stacks * (damagePerStack.Value + levelSteps * damagePerStackPerLevelStep.Value);
            }
        }
    }
}
