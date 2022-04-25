using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HLRiptide;
using HLRiptide.NetworkedObjects;

public class ServerSpawnController : NetworkedBehaviour
{
    public GameObject playerGameObject;

    Transform[] spawnPositons = new Transform[1];

    public override void OnServerStart()
    {
        Spawn[] spawns = FindObjectsOfType<Spawn>();

        spawnPositons = new Transform[spawns.Length];
        for (int i = 0; i < spawns.Length; i++) spawnPositons[i] = spawns[i].transform;
    }

    public override void OnServerClientFinishConnecting(ushort id)
    {
        GameObject go = Instantiate(playerGameObject);

        go.transform.position = new Vector3(0, 3, 0);

        SetPlayerSpawn(go, 0);

        go.GetComponent<NetworkedObject>().SpawnOnNetwork(id);
    }

    private void SetPlayerSpawn(GameObject player, int spawnindex)
    {
        player.transform.position = spawnPositons[spawnindex].position;
        player.transform.rotation = spawnPositons[spawnindex].rotation;
    }

    public override void OnRegisterCommands()
    {
        
    }
}
