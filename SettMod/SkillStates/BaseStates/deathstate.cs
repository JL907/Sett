﻿using EntityStates;
using RoR2;
using UnityEngine;

namespace SettMod.SkillStates.BaseStates
{
    internal class Death : GenericCharacterDeath
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound("Death", base.gameObject);
            Animator modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                modelAnimator.CrossFadeInFixedTime("Death", 0.1f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}