using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace CF.Game
{
	public class SpawnRequest
	{
		public Vector3? Position;
		public Quaternion? Rotation;
	}

	public class NetCharacterSpawner : NetworkBehaviour
    {
		public override void Spawned()
		{
			if(Runner.IsServer)
			{
				NetGame.Server.Spawner = this;
			}
		}		
    }
}
