using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace CF.Game
{
    public class NetGame : SimulationBehaviour, ISpawned
    {
        public new static NetworkRunner NetRunner;
		public static NetPlayerManager PlayerManager;
		public static NetProximityChat ProximityChat;

		public class Client
		{
            public static NetPlayerObject PlayerObject;            
		}

		public class Server
        {
            public static NetCharacterSpawner Spawner;
			public static HashSet<NetPlayerObject> PlayerObjects;
			public static HashSet<NetworkObject> AOIObjects;
        }

		public void Spawned()
		{
			NetRunner = Runner;

			if (NetRunner.IsServer)
			{
				Server.PlayerObjects = new HashSet<NetPlayerObject>();
				Server.AOIObjects = new HashSet<NetworkObject>();
			}

			Debug.LogError("Spawned!");
		}
	}
}
