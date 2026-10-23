using EntityStates;
using RoR2;
using UnityEngine;

namespace SettMod.States
{
    public class SettMain : GenericCharacterMain
    {
        public LocalUser localUser;

        public override void OnEnter()
        {
            base.OnEnter();
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
    }
}