using System;
using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
	public class DeathState : GenericCharacterDeath
	{
		protected Animator animator;

		public override void OnEnter()
		{
			base.OnEnter();
			this.animator = base.GetModelAnimator();
			this.PlayDeathAnimation();
			this.PlayDeathSound();
		}

		// Token: 0x0600449E RID: 17566 RVA: 0x00004381 File Offset: 0x00002581
		public override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
		{
			base.PlayDeathAnimation();
            if (animator)
            {
				base.PlayCrossfade("Fullbody, Override", "Death", "Death.playbackRate", 3f, crossfadeDuration);
			}
		}
        public override void PlayDeathSound()
        {
            base.PlayDeathSound();
			if (base.sfxLocator)
			{
				Util.PlaySound("Death", base.gameObject);

			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}
