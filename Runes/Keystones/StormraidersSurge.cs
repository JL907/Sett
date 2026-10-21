using BepInEx.Configuration;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Runes
{
    /// <summary>
    /// Dealing a chunk of an enemy's max health within a short window grants a burst of movement speed
    /// and slow resistance. Replaced Phase Rush in League, so it takes over Phase Rush's loadout slot.
    /// </summary>
    internal sealed class StormraidersSurge : Keystone
    {
        internal static ConfigEntry<float> damageThreshold;
        internal static ConfigEntry<float> window;
        internal static ConfigEntry<float> duration;
        internal static ConfigEntry<float> speed;
        internal static ConfigEntry<float> slowResistance;
        internal static ConfigEntry<float> cooldownStart;
        internal static ConfigEntry<float> cooldownEnd;
        internal static ConfigEntry<int> cooldownEndLevel;

        internal static BuffDef speedBuff;
        internal static BuffDef cooldownBuff;

        public override KeystoneType Type => KeystoneType.StormraidersSurge;
        // Kept from Phase Rush (Riot did the same) so saved loadouts keep this pick.
        public override string Id => "PHASE_RUSH";
        public override string DisplayName => "Stormraider's Surge";
        public override string IconName => "Stormraiders_Surge_rune";

        // Counts every point of damage, including damage-over-time ticks.
        internal override bool RequiresProcCoefficient => false;

        internal override void BindConfig(ConfigFile config)
        {
            // BepInEx doesn't allow apostrophes in section names.
            const string section = "Stormraiders Surge";
            damageThreshold = config.Bind(section, "Damage Threshold", 0.25f, "Damage to one enemy, as a fraction of its max health (plus shield), needed to trigger.");
            window = config.Bind(section, "Damage Window", 3f, "Seconds the damage has to be dealt in.");
            duration = config.Bind(section, "Duration", 4f, "");
            speed = config.Bind(section, "Movement Speed", 0.48f, "Fraction of movement speed added.");
            slowResistance = config.Bind(section, "Slow Resistance", 0.5f, "Fraction of incoming slows ignored while active.");
            cooldownStart = config.Bind(section, "Cooldown At Level 1", 20f, "");
            cooldownEnd = config.Bind(section, "Cooldown At Max Level", 10f, "");
            cooldownEndLevel = config.Bind(section, "Max Cooldown Level", 18, "Level at which the cooldown reaches its lowest value. It scales evenly from level 1.");
        }

        internal override void CreateBuffs()
        {
            speedBuff = KeystoneContent.AddBuff("KeystoneStormraidersSurgeBuff", icon, false, false);
            cooldownBuff = KeystoneContent.AddBuff("KeystoneStormraidersSurgeCooldown", icon, false, false);
            cooldownBuff.buffColor = new Color(0.45f, 0.45f, 0.45f);
            cooldownBuff.isCooldown = true;
        }

        internal override string Description()
        {
            return "Dealing damage equal to <color=#f68835>" + Pct(damageThreshold.Value) + " of an enemy's max health</color> within " +
                Num(window.Value) + " seconds grants <color=#f68835>" + Pct(speed.Value) + "</color> bonus <color=#c9aa71>movement speed</color> and " +
                "<color=#c9aa71>" + Pct(slowResistance.Value) + " slow resistance</color> for " + Num(duration.Value) + " seconds. " +
                "Cooldown: " + Num(cooldownStart.Value) + " - " + Num(cooldownEnd.Value) + " seconds (based on level).";
        }

        internal override void OnHitServer(KeystoneHandler handler, DamageReport damageReport)
        {
            CharacterBody body = handler.body;
            HealthComponent victim = damageReport.victimBody.healthComponent;
            if (!victim || damageReport.damageDealt <= 0f) return;

            SurgeTracker tracker = body.GetComponent<SurgeTracker>();
            if (!tracker) tracker = body.gameObject.AddComponent<SurgeTracker>();

            float total = tracker.AddDamage(victim, damageReport.damageDealt, window.Value);
            if (body.HasBuff(cooldownBuff) || body.HasBuff(speedBuff)) return;

            if (total >= victim.fullCombinedHealth * damageThreshold.Value)
            {
                tracker.Clear();
                body.AddTimedBuff(speedBuff, duration.Value);
                body.AddTimedBuff(cooldownBuff, Cooldown(body.level));
            }
        }

        internal override void RecalculateStats(CharacterBody body, float levelSteps)
        {
            if (!body.HasBuff(speedBuff)) return;

            body.moveSpeed += body.moveSpeed * speed.Value;

            // Vanilla divides speed by (1 + sum of active slows). Undo the resisted share of it.
            float resist = Mathf.Clamp01(slowResistance.Value);
            float slow = TotalSlow(body);
            if (resist > 0f && slow > 0f)
            {
                body.moveSpeed *= (1f + slow) / (1f + slow * (1f - resist));
            }
        }

        private static float Cooldown(float level)
        {
            int endLevel = Mathf.Max(2, cooldownEndLevel.Value);
            float t = Mathf.Clamp01((level - 1f) / (endLevel - 1f));
            return Mathf.Lerp(cooldownStart.Value, cooldownEnd.Value, t);
        }

        // Vanilla slow buffs and how much each adds to the slow divisor. Looked up by name so a buff
        // missing from this game version is simply skipped.
        private static readonly string[] slowBuffNames = { "bdSlow30", "bdSlow50", "bdSlow60", "bdSlow80", "bdClayGoo", "bdCripple", "bdJailerSlow" };
        private static readonly float[] slowBuffAmounts = { 0.3f, 0.5f, 0.6f, 0.8f, 0.5f, 1f, 1f };
        private static BuffIndex[] slowBuffs;

        private static float TotalSlow(CharacterBody body)
        {
            if (slowBuffs == null)
            {
                slowBuffs = new BuffIndex[slowBuffNames.Length];
                for (int i = 0; i < slowBuffNames.Length; i++)
                {
                    slowBuffs[i] = BuffCatalog.FindBuffIndex(slowBuffNames[i]);
                }
            }

            float total = 0f;
            for (int i = 0; i < slowBuffs.Length; i++)
            {
                if (slowBuffs[i] != BuffIndex.None && body.HasBuff(slowBuffs[i])) total += slowBuffAmounts[i];
            }
            return total;
        }
    }

    /// <summary>Server side, on the attacker: recent damage per enemy for Stormraider's Surge.</summary>
    public class SurgeTracker : MonoBehaviour
    {
        private struct Hit
        {
            public float time;
            public float damage;
        }

        private readonly Dictionary<HealthComponent, List<Hit>> hits = new Dictionary<HealthComponent, List<Hit>>();
        private readonly List<HealthComponent> stale = new List<HealthComponent>();

        /// <summary>Records damage to a victim and returns the total dealt to it within the window.</summary>
        internal float AddDamage(HealthComponent victim, float damage, float window)
        {
            float now = Time.fixedTime;

            stale.Clear();
            foreach (KeyValuePair<HealthComponent, List<Hit>> pair in hits)
            {
                pair.Value.RemoveAll(h => now - h.time > window);
                if (pair.Value.Count == 0 || !pair.Key) stale.Add(pair.Key);
            }
            foreach (HealthComponent key in stale) hits.Remove(key);

            List<Hit> list;
            if (!hits.TryGetValue(victim, out list))
            {
                list = new List<Hit>();
                hits[victim] = list;
            }
            list.Add(new Hit { time = now, damage = damage });

            float total = 0f;
            foreach (Hit hit in list) total += hit.damage;
            return total;
        }

        internal void Clear()
        {
            hits.Clear();
        }
    }
}
