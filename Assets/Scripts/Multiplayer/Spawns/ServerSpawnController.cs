using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HLRiptide;
using HLRiptide.NetworkedObject;

public class ServerSpawnController : NetworkedBehaviour
{
    public GameObject playerGameObject;

    Transform[] spawnPositons;

    public override void OnServerStart()
    {
        Spawn[] spawns = FindObjectsOfType<Spawn>();

        spawnPositons = new Transform[spawns.Length];
        for (int i = 0; i < spawns.Length; i++) spawnPositons[i] = spawns[i].transform;
    }

    public override void OnServerClientFinishConnecting(ushort id)
    {
        GameObject go = Instantiate(playerGameObject);

        SetPlayerSpawn(go, new System.Random().Next(0, spawnPositons.Length));

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