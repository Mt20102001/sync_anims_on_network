using Fusion;
using System;
using UnityEngine;

namespace CF.Game
{
    public class NetCharacterStats : NetworkBehaviour
    {
		[Networked] public PlayerId PlayerId { get; set; }
		[Networked] public Guid PlayerGuid { get; set;  }
		
		public Vector3 CameraPosition { get; set; }
		public Vector3 CameraDirection { get; set; }

		public System.Action OnNameChanged;

		public override void Spawned()
		{
			//name = PlayerId.ToX16String();
			name = PlayerGuid.ToString();
			OnNameChanged?.Invoke();
		}
	}
}
