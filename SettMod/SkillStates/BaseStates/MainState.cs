using RoR2;
using EntityStates;
using UnityEngine;
using System;

namespace SettMod.States
{
    public class SettMain : GenericCharacterMain
    {
        private ChildLocator childLocator;
        private Animator animator;
        public LocalUser localUser;

        public override void OnEnter()
        {
            base.OnEnter();
            this.childLocator = base.GetModelChildLocator();
            this.animator = base.GetModelAnimator();
            this.localUser = LocalUserManager.readOnlyLocalUsersList[0];
        }

        public override void Update()
        {
            base.Update();

            if (base.isAuthority && base.characterMotor.isGrounded && !this.localUser.isUIFocused)
            {
                if (Input.GetKeyDown(Modules.Config.tauntKeybind.Value))
                {
                    this.outer.SetInterruptState(new Emotes.Taunt(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.jokeKeybind.Value))
                {
                    this.outer.SetInterruptState(new Emotes.Joke(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.laughKeybind.Value))
                {
                    this.outer.SetInterruptState(new Emotes.Laugh(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.danceKeybind.Value))
                {
                    this.outer.SetInterruptState(new Emotes.Dance(), InterruptPriority.Any);
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}