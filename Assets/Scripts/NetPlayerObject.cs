using Fusion;
using System;
using UnityEngine;

namespace CF.Game
{
    public class NetPlayerObject : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef characterPrefab;
        [SerializeField] private NetworkObject[] abilityPrefabs;

        [Networked] public NetworkObject Character { get; private set; }

		public Vector3 Position => Character == null ? Vector3.zero : Character.transform.position;

		public Guid playerGuid;	

		public PlayerId playerId;

		public override void Spawned()
		{
			Runner.SetPlayerObject(Object.InputAuthority, Object);

			if(Object.HasInputAuthority)
			{
				NetGame.Client.PlayerObject = this;
			}

			if (Runner.IsServer)
			{
				var charObject = Runner.Spawn(characterPrefab, inputAuthority: Object.InputAuthority, onBeforeSpawned: (runner, no) =>
				{
					no.transform.position = Vector3.up;

					var stats = no.GetBehaviour<NetCharacterStats>();
					stats.PlayerId = playerId;
					stats.PlayerGuid = playerGuid;

                    // Add Abilities
                    if (abilityPrefabs != null && abilityPrefabs.Length > 0)
                    {
                        //Debug.Log("Try get ExpertPlayerAbilities");
                        
                        //else
                        //{
                        //    Debug.Log("Cannot Find ExpertPlayerAbilities");
                        //}
                    }
                });

				Character = charObject;

				NetGame.Server.PlayerObjects.Add(this);
			}
		}

		//[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
		//private void RpcAuthenticate(string token)
		//{
		//	Debug.Log($"Client send token: {token}");

		//	var data = ValidateNakamaToken(token, "Ugug2uTEkoKzhk7u5vmS6xOoRBtoiZ6KIcwbbSuQLTf0H4f5");

		//	Debug.Log("PlayerId: " + data.uid);

		//	//try
		//	//{
		//	//	string s = JWT.JsonWebToken.Decode(token, "Ugug2uTEkoKzhk7u5vmS6xOoRBtoiZ6KIcwbbSuQLTf0H4f5");

		//	//	Debug.Log(s);
		//	//}
		//	//catch(System.Exception e)
		//	//{
		//	//	Debug.Log(e.Message);
		//	//}			
		//}
	}
}
