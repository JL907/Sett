using BepInEx.Configuration;
using RoR2;
using System.Globalization;
using UnityEngine;

namespace Runes
{
    public enum KeystoneType
    {
        None = 0,
        Conqueror = 1,
        LethalTempo = 2,
        StormraidersSurge = 4,
        Electrocute = 8,
        GraspOfTheUndying = 16,
        GlacialAugment = 32
    }

    /// <summary>
    /// One keystone: its config, buffs, loadout text and gameplay. Each keystone lives in its own file
    /// and is registered in <see cref="KeystoneContent.keystones"/>.
    /// </summary>
    internal abstract class Keystone
    {
        protected const string Hit = "<color=#c9aa71>Hits with a proc coefficient</color>";

        public abstract KeystoneType Type { get; }

        /// <summary>Used for the skill name and tokens (KEYSTONE_&lt;Id&gt;). Don't change it, saved loadouts depend on it.</summary>
        public abstract string Id { get; }

        public abstract string DisplayName { get; }

        /// <summary>Embedded icon file name in Icons/, without the extension.</summary>
        public abstract string IconName { get; }

        internal Sprite icon;

        internal abstract void BindConfig(ConfigFile config);

        /// <summary>Called once <see cref="icon"/> is loaded. Create buffs with KeystoneContent.AddBuff.</summary>
        internal virtual void CreateBuffs() { }

        /// <summary>Called after CreateBuffs. Build effect prefabs with KeystoneContent.CreatePrefab and register them with AddEffect.</summary>
        internal virtual void CreateEffects() { }

        internal abstract string Description();

        /// <summary>When true (the default), only hits with a proc coefficient above 0 reach OnHitServer, so DoT ticks are ignored.</summary>
        internal virtual bool RequiresProcCoefficient => true;

        /// <summary>Called from the plugin's Awake for keystones that need game hooks. Undo them in Unhook.</summary>
        internal virtual void Hook() { }

        internal virtual void Unhook() { }

        /// <summary>Server only. Damage dealt by a body with this keystone selected (see RequiresProcCoefficient).</summary>
        internal virtual void OnHitServer(KeystoneHandler handler, DamageReport damageReport) { }

        /// <summary>Server only. Called every FixedUpdate while this keystone is selected.</summary>
        internal virtual void FixedUpdateServer(KeystoneHandler handler) { }

        /// <summary>Server only. A body with this keystone selected killed an enemy.</summary>
        internal virtual void OnKillServer(KeystoneHandler handler, DamageReport damageReport) { }

        /// <summary>Runs for every body after vanilla stats, on server and clients. Read buff counts here.</summary>
        internal virtual void RecalculateStats(CharacterBody body, float levelSteps) { }

        protected static string Num(float v) { return v.ToString("0.###", CultureInfo.InvariantCulture); }
        protected static string Pct(float v) { return Num(v * 100f) + "%"; }
    }
}
