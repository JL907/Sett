using EntityStates;
using R2API;
using SettMod.Modules;
using SettMod.SkillStates.Emotes;
using UnityEngine;

namespace SettMod.States.Emotes
{
    public class Joke : BaseEmote
    {
        private GameObject mom;
        public override void OnEnter()
        {
            this.animString = "Joke";
            this.duration = 8.667f;
            this.soundString = "SettJoke";
            base.OnEnter();
            if(!this.mom) this.mom = UnityEngine.Object.Instantiate<GameObject>(Assets.mainAssetBundle.LoadAsset<GameObject>("momPortrait"));
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.mom)
            {
                this.mom.transform.parent = this.FindModelChild("L_Hand").transform;
                this.mom.transform.position = this.FindModelChild("L_Hand").transform.position;
                this.mom.transform.localPosition = new Vector3(0.1906874f, 0.1412978f, -0.1224022f);
                this.mom.transform.localRotation = Quaternion.Euler(-176.593f, 85.119f, 9.455994f);
            }
        }
        public override void OnExit()
        {
            if (this.mom) EntityState.Destroy(this.mom.gameObject);
            base.OnExit();
        }
    }
}