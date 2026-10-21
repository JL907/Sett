using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Keystones
{
    /// <summary>
    /// Public entry points for other survivor mods. Add
    /// [BepInDependency(Keystones.KeystonePlugin.GUID)] to your plugin and call these from Awake,
    /// before content packs are assigned.
    /// </summary>
    public static class KeystoneAPI
    {
        internal static readonly HashSet<string> registered = new HashSet<string>();
        internal static readonly HashSet<string> excluded = new HashSet<string>();

        /// <summary>
        /// Always give this survivor a Keystone slot, even when "Enable For All Survivors" is off.
        /// Players can still switch it off per survivor in the config.
        /// </summary>
        public static void RegisterSurvivor(GameObject bodyPrefab)
        {
            if (!bodyPrefab) return;
            WarnIfLate(bodyPrefab.name);
            registered.Add(bodyPrefab.name);
        }

        /// <summary>Never give this survivor a Keystone slot (e.g. its kit already has one).</summary>
        public static void ExcludeSurvivor(GameObject bodyPrefab)
        {
            if (!bodyPrefab) return;
            WarnIfLate(bodyPrefab.name);
            excluded.Add(bodyPrefab.name);
        }

        /// <summary>The keystone this body currently has selected, or None.</summary>
        public static KeystoneType GetKeystone(CharacterBody body)
        {
            if (!body) return KeystoneType.None;
            KeystoneHandler handler = body.GetComponent<KeystoneHandler>();
            return handler ? handler.CurrentKeystone : KeystoneType.None;
        }

        public static bool HasKeystoneSlot(CharacterBody body)
        {
            return body && body.GetComponent<KeystoneHandler>();
        }

        public static BuffDef ConquerorBuff => KeystoneContent.conquerorBuff;
        public static BuffDef LethalTempoBuff => KeystoneContent.lethalBuff;
        public static BuffDef PhaseRushDebuff => KeystoneContent.phaseRushDebuff;
        public static BuffDef PhaseRushSpeedBuff => KeystoneContent.phaseRushSpeedBuff;
        public static BuffDef ElectrocuteDebuff => KeystoneContent.electrocuteDebuff;

        private static void WarnIfLate(string bodyName)
        {
            if (KeystonePlugin.slotsInjected)
            {
                KeystonePlugin.Log.LogWarning("KeystoneAPI called for " + bodyName + " after survivors were already set up; call it from your plugin's Awake instead.");
            }
        }
    }
}
