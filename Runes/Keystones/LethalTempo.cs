using BepInEx.Configuration;
using RoR2;

namespace Runes
{
    internal sealed class LethalTempo : Keystone
    {
        internal static ConfigEntry<int> maxStacks;
        internal static ConfigEntry<float> duration;
        internal static ConfigEntry<float> attackSpeedPerStack;

        internal static BuffDef buff;

        public override KeystoneType Type => KeystoneType.LethalTempo;
        public override string Id => "LETHAL_TEMPO";
        public override string DisplayName => "Lethal Tempo";
        public override string IconName => "Lethal_Tempo_rune";

        internal override void BindConfig(ConfigFile config)
        {
            const string section = "Lethal Tempo";
            maxStacks = config.Bind(section, "Max Stacks", 6, "");
            duration = config.Bind(section, "Stack Duration", 6f, "Seconds without hitting anything before stacks start falling off.");
            attackSpeedPerStack = config.Bind(section, "Attack Speed Per Stack", 0.10f, "Fraction of attack speed added per stack.");
        }

        internal override void CreateBuffs()
        {
            buff = KeystoneContent.AddBuff("KeystoneLethalTempoBuff", icon, true, false);
        }

        internal override string Description()
        {
            return Hit + " against enemies grant <color=#ffffff>1</color> stack of Lethal Tempo, up to " + maxStacks.Value + " stacks. " +
                "Gain <color=#f68835>" + Pct(attackSpeedPerStack.Value) + "</color> bonus attack speed per stack, up to " +
                "<color=#f68835>" + Pct(attackSpeedPerStack.Value * maxStacks.Value) + "</color> at maximum stacks.";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            handler.AddStack(buff, maxStacks.Value);
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
                body.attackSpeed += body.attackSpeed * stacks * attackSpeedPerStack.Value;
            }
        }
    }
}
