using SettMod.SkillStates.Emotes;

namespace SettMod.States.Emotes
{
    public class Taunt : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Taunt";
            this.duration = 6.417f;
            this.soundString = "SettTaunt";

            base.OnEnter();
        }
    }
}