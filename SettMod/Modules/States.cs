using SettMod.SkillStates;
using SettMod.SkillStates.BaseStates;
using SettMod.States;
using SettMod.States.Emotes;
using System;
using System.Collections.Generic;

namespace SettMod.Modules
{
    public static class States
    {
        internal static List<Type> entityStates = new List<Type>();

        internal static void RegisterStates()
        {
            entityStates.Add(typeof(KnuckleDown));

            entityStates.Add(typeof(Facebreaker));

            entityStates.Add(typeof(Dash));

            entityStates.Add(typeof(SettMain));

            entityStates.Add(typeof(HayMaker));

            entityStates.Add(typeof(Death));

            entityStates.Add(typeof(Taunt));

            entityStates.Add(typeof(Joke));

            entityStates.Add(typeof(Laugh));

            entityStates.Add(typeof(Dance));
        }
    }
}