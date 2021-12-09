using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SettMod.SkillStates.Keystone
{
    public class ElectrocuteHandler : MonoBehaviour
    {
        public float stopwatch = 0.0f;
        public float duration = 10f;

        private void FixedUpdate()
        {
            if (stopwatch < duration)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            if (stopwatch >= duration)
            {
                Destroy(this);
            }
        }
    }
}
