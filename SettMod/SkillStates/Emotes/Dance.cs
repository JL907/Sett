using SettMod.SkillStates.Emotes;

namespace SettMod.States.Emotes
{
    public class Dance : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Dance";
            this.duration = 6.417f;

            base.OnEnter();
        }
    }
}