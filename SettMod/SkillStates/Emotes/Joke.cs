using SettMod.SkillStates.Emotes;

namespace SettMod.States.Emotes
{
    public class Joke : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Joke";
            this.duration = 8.667f;
            this.soundString = "SettJoke";

            base.OnEnter();
        }
    }
}