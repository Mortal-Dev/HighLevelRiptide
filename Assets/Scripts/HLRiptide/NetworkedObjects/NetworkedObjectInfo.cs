using UnityEngine;

namespace HLRiptide.NetworkedObjects
{
    public class NetworkedObjectInfo
    {
        public uint id;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public NetworkedObjectInfo(uint id, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.id = id;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }
}
