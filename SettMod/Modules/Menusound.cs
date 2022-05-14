﻿using RoR2;
using UnityEngine;

namespace SettMod
{
    public class MenuSound : MonoBehaviour
    {
        private uint playID;
        private uint playID2;

        private void OnDestroy()
        {
            if (this.playID != 0) AkSoundEngine.StopPlayingID(this.playID);
            if (this.playID2 != 0) AkSoundEngine.StopPlayingID(this.playID2);
        }

        private void OnEnable()
        {
            this.Invoke("PlayEffect", 0.05f);
        }

        private void PlayEffect()
        {
            if (Modules.Config.voiceLines.Value)
            {
                this.playID = Util.PlaySound("SettMenuVO", base.gameObject);
            }
            this.playID2 = Util.PlaySound("SettMenuSFX", base.gameObject);
        }
    }
}