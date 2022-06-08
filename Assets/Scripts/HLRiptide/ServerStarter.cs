using HLRiptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerStarter : MonoBehaviour
{
    public bool isServer;

    void Start()
    {
        if (isServer) NetworkManager.Singleton.StartServer(2700, 2, 1);
    }

}
