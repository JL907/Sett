using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Runes
{
    public static class KeystoneAPI
    {
        internal static readonly HashSet<string> registered = new HashSet<string>();
        internal static readonly HashSet<string> excluded = new HashSet<string>();

        public static void RegisterSurvivor(GameObject bodyPrefab)
        {
            if (!bodyPrefab) return;
            WarnIfLate(bodyPrefab.name);
            registered.Add(bodyPrefab.name);
        }

        public static void ExcludeSurvivor(GameObject bodyPrefab)
        {
            if (!bodyPrefab) return;
            WarnIfLate(bodyPrefab.name);
            excluded.Add(bodyPrefab.name);
        }

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

        public static BuffDef ConquerorBuff => Conqueror.buff;
        public static BuffDef LethalTempoBuff => LethalTempo.buff;
        public static BuffDef StormraidersSurgeBuff => StormraidersSurge.speedBuff;
        public static BuffDef ElectrocuteBuff => Electrocute.buff;
        public static BuffDef GraspReadyBuff => GraspOfTheUndying.readyBuff;
        public static BuffDef GraspHealthBuff => GraspOfTheUndying.healthBuff;

        private static void WarnIfLate(string bodyName)
        {
            if (KeystonePlugin.slotsInjected)
            {
                KeystonePlugin.Log.LogWarning("KeystoneAPI called for " + bodyName + " after survivors were already set up; call it from your plugin's Awake instead.");
            }
        }
    }
}
