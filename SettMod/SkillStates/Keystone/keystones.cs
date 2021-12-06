using System;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace SettMod.SkillStates.Keystone
{
    public class KeyStoneHandler : NetworkBehaviour
    {
        [FormerlySerializedAs("Keystone")]
        public GenericSkill keyStone;
    }


}
