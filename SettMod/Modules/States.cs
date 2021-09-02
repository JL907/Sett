using SettMod.SkillStates;
using SettMod.SkillStates.BaseStates;
using System;
using System.Collections.Generic;

namespace SettMod.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        internal static void RegisterStates()
        {
            entityStates.Add(typeof(BaseMeleeAttack));

            entityStates.Add(typeof(Facebreaker2));

            entityStates.Add(typeof(Roll));

            entityStates.Add(typeof(HayMaker));
        }
    }
}