using HLRiptide.NetworkedCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.Networks.NetworkSceneManagers
{
    public abstract class BaseNetworkSceneManager
    {
        public InternalNetworkedCommand<int> syncClientSceneToServerScene;
        public InternalNetworkedCommand<ushort> clientFinishedLoadingSceneCommand;

        public void AwakeNetworkedBehaviours()
        {
            NetworkedBehaviour[] networkedBehaviours = UnityEngine.Object.FindObjectsOfType(typeof(NetworkedBehaviour)) as NetworkedBehaviour[];

            foreach (NetworkedBehaviour networkedBehaviour in networkedBehaviours) networkedBehaviour.InternalAwake();
        }
    }
}
