using CF.Game;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : NetworkBehaviour
{
    public HashSet<NetPlayerObject> Players = new HashSet<NetPlayerObject>();

    [SerializeField] private NetPlayerObject playerObjectPrefab;

    public override void Spawned()
    {
        var events = Runner.GetBehaviour<NetworkEvents>();

        events.PlayerJoined.AddListener(OnPlayerJoined);
        events.PlayerLeft.AddListener(OnPlayerLeft);

        //Runner.LagCompensation.Posi
    }

    private void OnPlayerLeft(NetworkRunner arg0, PlayerRef arg1)
    {
        throw new NotImplementedException();
    }

    private void OnPlayerJoined(NetworkRunner arg0, PlayerRef arg1)
    {
        throw new NotImplementedException();
    }
}
