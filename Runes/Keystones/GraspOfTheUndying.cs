using BepInEx.Configuration;
using RoR2;
using UnityEngine;

namespace Runes
{
    internal sealed class GraspOfTheUndying : Keystone
    {
        internal static ConfigEntry<float> cooldown;
        internal static ConfigEntry<float> damage;
        internal static ConfigEntry<float> heal;
        internal static ConfigEntry<float> healthPerStack;

        internal static BuffDef readyBuff;
        internal static BuffDef healthBuff;

        public override KeystoneType Type => KeystoneType.GraspOfTheUndying;
        public override string Id => "GRASP_OF_THE_UNDYING";
        public override string DisplayName => "Grasp of the Undying";
        public override string IconName => "Grasp_of_the_Undying_rune";

        internal override void BindConfig(ConfigFile config)
        {
            const string section = "Grasp of the Undying";
            cooldown = config.Bind(section, "Cooldown", 4f, "Seconds after an empowered hit before the next one is ready.");
            damage = config.Bind(section, "Damage Max Health Fraction", 0.05f, "Bonus damage on an empowered hit, as a fraction of your max health.");
            heal = config.Bind(section, "Heal Max Health Fraction", 0.02f, "Healing on an empowered hit, as a fraction of your max health.");
            healthPerStack = config.Bind(section, "Max Health Per Proc", 1f, "Permanent max health gained per empowered hit. Lasts for the rest of the run.");
        }

        internal override void CreateBuffs()
        {
            readyBuff = KeystoneContent.AddBuff("KeystoneGraspReadyBuff", icon, false, false);
            healthBuff = KeystoneContent.AddBuff("KeystoneGraspHealthBuff", icon, true, false);
        }

        internal override string Description()
        {
            return "Every <color=#ffffff>" + Num(cooldown.Value) + "</color> seconds, your next " +
                "<color=#c9aa71>hit with a proc coefficient</color> against an enemy deals bonus damage equal to " +
                "<color=#f68835>" + Pct(damage.Value) + " of your max health</color>, " +
                "<color=#c9aa71>heals</color> you for <color=#008744>" + Pct(heal.Value) + " of your max health</color>, " +
                "and permanently increases your <color=#c9aa71>max health</color> by <color=#008744>" + Num(healthPerStack.Value) + "</color> for the rest of the run.";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            CharacterBody body = handler.body;
            if (!body.HasBuff(readyBuff)) return;

            body.RemoveBuff(readyBuff);
            GraspTracker tracker = GetTracker(body);
            if (tracker) tracker.stopwatch = 0f;

            float maxHealth = handler.healthComponent ? handler.healthComponent.fullHealth : body.maxHealth;

            CharacterBody victim = damageReport.victimBody;
            HealthComponent victimHealth = victim.healthComponent;
            if (victimHealth && victimHealth.alive)
            {
                victimHealth.TakeDamage(new DamageInfo
                {
                    attacker = body.gameObject,
                    inflictor = body.gameObject,
                    damage = maxHealth * damage.Value,
                    crit = false,
                    position = victim.corePosition,
                    force = Vector3.zero,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0f,
                    damageColorIndex = DamageColorIndex.Item
                });
            }

            if (handler.healthComponent)
            {
                handler.healthComponent.Heal(maxHealth * heal.Value, default(ProcChainMask), true);
            }

            if (tracker)
            {
                tracker.stacks++;
                body.SetBuffCount(healthBuff.buffIndex, tracker.stacks);
            }
            else
            {
                body.AddBuff(healthBuff);
            }
        }

        internal override void FixedUpdateServer(KeystoneHandler handler)
        {
            CharacterBody body = handler.body;
            GraspTracker tracker = GetTracker(body);
            if (!tracker) return;

            if (tracker.body != body)
            {
                tracker.body = body;
                tracker.stopwatch = 0f;
                if (tracker.stacks > 0)
                {
                    body.SetBuffCount(healthBuff.buffIndex, tracker.stacks);
                }
            }

            if (body.HasBuff(readyBuff)) return;

            tracker.stopwatch += Time.fixedDeltaTime;
            if (tracker.stopwatch >= cooldown.Value)
            {
                body.AddBuff(readyBuff);
            }
        }

        internal override void Hook()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
        }

        internal override void Unhook()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients -= GetStatCoefficients;
        }

        // Added through R2API rather than after RecalculateStats: the game caps current health to the max it
        // calculated, so health added afterwards was cut off every time stats updated at full health.
        // As flat base health, it works like Bison Steak (counts toward health multipliers and curse).
        private static void GetStatCoefficients(CharacterBody body, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!body || !healthBuff) return;
            int stacks = body.GetBuffCount(healthBuff);
            if (stacks > 0)
            {
                args.baseHealthAdd += stacks * healthPerStack.Value;
            }
        }

        private static GraspTracker GetTracker(CharacterBody body)
        {
            CharacterMaster master = body.master;
            if (!master) return null;
            GraspTracker tracker = master.GetComponent<GraspTracker>();
            if (!tracker) tracker = master.gameObject.AddComponent<GraspTracker>();
            return tracker;
        }
    }

    public class GraspTracker : MonoBehaviour
    {
        public int stacks;
        internal float stopwatch;
        internal CharacterBody body;
    }
}
