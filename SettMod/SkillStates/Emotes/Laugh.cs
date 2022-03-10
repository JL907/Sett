using SettMod.SkillStates.Emotes;

namespace SettMod.States.Emotes
{
    public class Laugh : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Laugh";
            this.duration = 3.667f;
            this.soundString = "SettLaugh";

            base.OnEnter();
        }
    }
}