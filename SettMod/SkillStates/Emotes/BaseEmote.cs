﻿using EntityStates;
using RoR2;
using SettMod.States.Emotes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SettMod.SkillStates.Emotes
{
    public class BaseEmote : BaseState
    {
        public string soundString;
        public string animString;
        public float duration;
        public float animDuration;
        public bool normalizeModel;

        private uint activePlayID;
        private Animator animator;
        protected ChildLocator childLocator;
        private CameraTargetParams.CameraParamsOverrideHandle handle;
        public LocalUser localUser;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.childLocator = base.GetModelChildLocator();
            this.localUser = LocalUserManager.readOnlyLocalUsersList[0];

            base.characterBody.hideCrosshair = true;

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = false;
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 0);
            this.animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 0);

            if (this.animDuration == 0 && this.duration != 0) this.animDuration = this.duration;

            if (this.duration > 0) base.PlayAnimation("FullBody, Override", this.animString, "Emote.playbackRate", this.duration);
            else base.PlayAnimation("FullBody, Override", this.animString, "Emote.playbackRate", this.animDuration);

            if (Modules.Config.voiceLines.Value) this.activePlayID = Util.PlaySound(soundString, base.gameObject);

            if (this.normalizeModel)
            {
                if (base.modelLocator)
                {
                    base.modelLocator.normalizeToFloor = true;
                }
            }

            handle = Modules.CameraParams.OverrideSettCameraParams(base.cameraTargetParams, SettCameraParams.EMOTE, 0.5f);
        }

        public override void OnExit()
        {
            base.OnExit();

            base.cameraTargetParams.RemoveParamsOverride(handle, 0.2f);

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = true;
            if (this.animator)
            {
                this.animator.SetLayerWeight(animator.GetLayerIndex("AimPitch"), 1);
                this.animator.SetLayerWeight(animator.GetLayerIndex("AimYaw"), 1);
            }

            if (this.normalizeModel)
            {
                if (base.modelLocator)
                {
                    base.modelLocator.normalizeToFloor = false;
                }
            }

            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool flag = false;

            if (base.characterMotor)
            {
                if (!base.characterMotor.isGrounded) flag = true;
                if (base.characterMotor.velocity != Vector3.zero) flag = true;
            }

            if (base.inputBank)
            {
                if (base.inputBank.skill1.down) flag = true;
                if (base.inputBank.skill2.down) flag = true;
                if (base.inputBank.skill3.down) flag = true;
                if (base.inputBank.skill4.down) flag = true;
                if (base.inputBank.jump.down) flag = true;

                if (base.inputBank.moveVector != Vector3.zero) flag = true;
            }

            //emote cancels
            if (base.isAuthority && base.characterMotor.isGrounded && !this.localUser.isUIFocused)
            {
                if (Input.GetKeyDown(Modules.Config.tauntKeybind.Value))
                {
                    this.outer.SetInterruptState(new Taunt(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.jokeKeybind.Value))
                {
                    this.outer.SetInterruptState(new Joke(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.laughKeybind.Value))
                {
                    this.outer.SetInterruptState(new Laugh(), InterruptPriority.Any);
                    return;
                }
                else if (Input.GetKeyDown(Modules.Config.danceKeybind.Value))
                {
                    this.outer.SetInterruptState(new Dance { spam = this.outer.state is Dance ? true : false }, InterruptPriority.Any);
                    return;
                }
            }

            if (this.duration > 0 && base.fixedAge >= this.duration) flag = true;

            //if (this.animator) this.animator.SetBool("inCombat", true);

            if (flag)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}
