using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SettMod.SkillStates
{
    public class SyncSettGrab : INetMessage
    {
        private NetworkInstanceId grabberId;
        private NetworkInstanceId targetId;

        public SyncSettGrab() { }

        public SyncSettGrab(NetworkInstanceId grabberId, NetworkInstanceId targetId)
        {
            this.grabberId = grabberId;
            this.targetId = targetId;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.grabberId);
            writer.Write(this.targetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            this.grabberId = reader.ReadNetworkId();
            this.targetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject grabber = Util.FindNetworkObject(this.grabberId);
            GameObject target = Util.FindNetworkObject(this.targetId);
            SettGrabController.Apply(grabber, target);
        }
    }

    public class SyncSettRelease : INetMessage
    {
        private NetworkInstanceId targetId;

        public SyncSettRelease() { }

        public SyncSettRelease(NetworkInstanceId targetId)
        {
            this.targetId = targetId;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.targetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            this.targetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject target = Util.FindNetworkObject(this.targetId);
            if (!target) return;
            SettGrabController controller = target.GetComponent<SettGrabController>();
            if (controller) controller.Release();
        }
    }
}
