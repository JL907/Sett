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

    internal abstract class Keystone
    {
        protected const string Hit = "<color=#c9aa71>Hits with a proc coefficient</color>";

        public abstract KeystoneType Type { get; }

        public abstract string Id { get; }

        public abstract string DisplayName { get; }

        public abstract string IconName { get; }

        internal Sprite icon;

        internal abstract void BindConfig(ConfigFile config);

        internal virtual void CreateBuffs() { }

        internal virtual void CreateEffects() { }

        internal abstract string Description();

        internal virtual bool RequiresProcCoefficient => true;

        internal virtual void Hook() { }

        internal virtual void Unhook() { }

        internal virtual void OnHitServer(KeystoneHandler handler, DamageReport damageReport) { }

        internal virtual void FixedUpdateServer(KeystoneHandler handler) { }

        internal virtual void OnKillServer(KeystoneHandler handler, DamageReport damageReport) { }

        internal virtual void RecalculateStats(CharacterBody body, float levelSteps) { }

        protected static string Num(float v) { return v.ToString("0.###", CultureInfo.InvariantCulture); }
        protected static string Pct(float v) { return Num(v * 100f) + "%"; }
    }
}
