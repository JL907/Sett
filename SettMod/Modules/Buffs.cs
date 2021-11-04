using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace SettMod.Modules
{
    public static class Buffs
    {
        internal static List<BuffDef> buffDefs = new List<BuffDef>();

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
            regenBuff = AddNewBuff("SettRegenBuff", Resources.Load<Sprite>("textures/bufficons/texBuffRegenBoostIcon"), Color.green, true, false);
        }
    }
}