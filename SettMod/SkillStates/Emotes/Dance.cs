using SettMod.SkillStates.Emotes;
using UnityEngine.Networking;

namespace SettMod.States.Emotes
{
    public class Dance : BaseEmote
    {
        public bool spam;
        public override void OnEnter()
        {
            if (spam) this.animString = "DanceSpam";
            else this.animString = "Dance";
            this.duration = float.MaxValue;
            base.OnEnter();
        }
    }
}