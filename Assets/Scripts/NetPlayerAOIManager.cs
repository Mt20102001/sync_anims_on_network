using Fusion;
using UnityEngine;

namespace CF.Game
{
    public class NetPlayerAOIManager : NetworkBehaviour
    {
		public override void FixedUpdateNetwork()
		{
			foreach(var obj in NetGame.Server.PlayerObjects)
			{
				var charObj = obj.Character;

				//Runner.AddPlayerAreaOfInterest(charObj.InputAuthority, charObj.transform.position, 30f);
			}

			foreach(var playerObj in NetGame.Server.PlayerObjects)
			{
				var player = playerObj.Object.InputAuthority;
				var aoiObjects = NetGame.Server.AOIObjects;


				var stats = playerObj.Character.GetComponent<NetCharacterStats>();

				var camPos = stats.CameraPosition;
				var camDir = stats.CameraDirection;

				float cos = Mathf.Cos(10 * Mathf.Deg2Rad);

				foreach(var obj in aoiObjects)
				{
					obj.SetPlayerAlwaysInterested(player, true);
					if (obj.InputAuthority == player)
					{
						continue;
					}

					var objPos = obj.transform.position;

					var targetDir = obj.transform.position - camPos;

					var dot = Vector3.Dot(camDir, targetDir.normalized);

					if(dot > cos)
					{
						//inside
						//obj.SetPlayerAlwaysInterested(player, true);
						obj.SetPriority(player, 1);
					}
					else
					{
						//obj.SetPlayerAlwaysInterested(player, false);
						obj.SetPriority(player, 3);
					}
				}
			}
		}
    }
}
