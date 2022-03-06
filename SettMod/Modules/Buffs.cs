using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod.Modules
{
    public static class Buffs
    {
        internal static List<BuffDef> buffDefs = new List<BuffDef>();

        internal static BuffDef conquerorBuff;

        internal static BuffDef electrocuteDebuff;
        internal static BuffDef lethalBuff;
        internal static BuffDef movementSpeedBuff;

        internal static BuffDef phaseRushDebuff;

        // armor buff gained during roll
        internal static BuffDef regenBuff;

        // simple helper method
        internal static BuffDef AddNewBuff(string buffName, Sprite buffIcon, Color buffColor, bool canStack, bool isDebuff)
        {
            BuffDef buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = buffName;
            buffDef.buffColor = buffColor;
            buffDef.canStack = canStack;
            buffDef.isDebuff = isDebuff;
            buffDef.eliteDef = null;
            buffDef.iconSprite = buffIcon;
            buffDefs.Add(buffDef);
            return buffDef;
        }

        internal static void RegisterBuffs()
        {
            regenBuff = AddNewBuff("SettRegenBuff", RoR2.LegacyResourcesAPI.Load<Sprite>("textures/bufficons/texBuffRegenBoostIcon"), Color.green, true, false);

            conquerorBuff = AddNewBuff("ConquerorBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Conqueror_rune"), Color.white, true, false);

            lethalBuff = AddNewBuff("LethalBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Lethal_Tempo_rune"), Color.white, true, false);

            phaseRushDebuff = AddNewBuff("PhaseRushDebuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Phase_Rush_rune"), Color.white, true, true);

            electrocuteDebuff = AddNewBuff("ElectrocuteDebuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Electrocute_rune"), Color.white, true, true);

            movementSpeedBuff = AddNewBuff("MovementSpeedBuff", Modules.Assets.mainAssetBundle.LoadAsset<Sprite>("Phase_Rush_rune"), Color.white, true, false);
        }
    }
}