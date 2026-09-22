using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CF.Game
{
    public class NetProximityChat : NetworkBehaviour
    {
        private ReliableKey chatKey = ReliableKey.FromInts(2);

        private Dictionary<PlayerRef, int> playerCellPosition = new Dictionary<PlayerRef, int>();

        private Dictionary<int, HashSet<PlayerRef>> cellGroups = new Dictionary<int, HashSet<PlayerRef>>();

        private HashSet<PlayerRef> h1 = new HashSet<PlayerRef>();
        private HashSet<PlayerRef> h2 = new HashSet<PlayerRef>();

        private HashSet<PlayerRef> pendingUpdatePositions = new HashSet<PlayerRef>();

        private List<int> neighborCells = new List<int>();

		public override void Spawned()
		{
			NetGame.ProximityChat = this;

            var events = Runner.GetBehaviour<NetworkEvents>();

            if(Runner.IsServer)
            {
                events.OnReliableData.AddListener(OnServerReceiveReliableMessage);
            }
            
            if(Runner.IsPlayer)
            {
                events.OnReliableData.AddListener(OnClientReceiveReliableMessage);
            }
		}

        [ContextMenu("Check groups")]
        private void CheckGroups()
        {
            Debug.Log($"Connected player: {playerCellPosition.Count}");

            foreach(var kvp in cellGroups)
            {
                var id  = kvp.Key;
                var set = kvp.Value;

                Debug.Log($"Cell: {id}");

                foreach(var i in set)
                {
                    Debug.Log(i);
                }
            }            
        }

		public override void FixedUpdateNetwork()
		{
            //detect new player
            h1.Clear();
            h2.Clear();
            pendingUpdatePositions.Clear();


			foreach (var kvp in playerCellPosition)
            {
                h2.Add(kvp.Key);
			}

            foreach(var pref in Runner.ActivePlayers)
            {
                if (!Runner.TryGetPlayerObject(pref, out NetworkObject obj))
                    continue;

                h1.Add(pref);
                h2.Add(pref);

				bool contains = playerCellPosition.ContainsKey(pref);

				if (!contains)
				{
                    playerCellPosition[pref] = 0;
					pendingUpdatePositions.Add(pref);
				}
			}

            h2.ExceptWith(h1);

            //h2 now contains deleted elements
            foreach(var pref in h2)
            {
				playerCellPosition.Remove(pref);
                pendingUpdatePositions.Remove(pref);
            }

            foreach(var pref in Runner.ActivePlayers)
            {
				if (!Runner.TryGetPlayerObject(pref, out NetworkObject obj))
					continue;

                if (!obj.TryGetBehaviour(out NetPlayerObject pObject))
                    continue;

                int oldPos = playerCellPosition[pref];

                int newPos = PositionToCell(pObject.Position);

                if(oldPos != newPos || pendingUpdatePositions.Contains(pref))
                {
                    neighborCells.Clear();
                    GetNeighborCells(oldPos, neighborCells);

                    foreach(var c in neighborCells)
                    {
                        if(cellGroups.TryGetValue(c, out var set))
                        {
                            set.Remove(pref);
                        }
                    }

                    neighborCells.Clear();
                    GetNeighborCells(newPos, neighborCells);

                    foreach(var c in neighborCells)
                    {
                        if(!cellGroups.TryGetValue(c, out var set))
                        {
                            cellGroups[c] = set = new HashSet<PlayerRef>();
                        }

                        set.Add(pref);
                    }
                }
			}

            //Clear empty cellGroups
		}

		private int PositionToCell(Vector3 position)
        {
            return 0;
        }

        private void GetNeighborCells(int cell, List<int> neighborCellList)
        {
            neighborCellList.Add(cell);
        }

        public void SendChatMessage(string message)
        {
            int count = Encoding.UTF8.GetByteCount(message);

            Span<byte> dataBuffer = stackalloc byte[count];

            Encoding.UTF8.GetBytes(message, dataBuffer);

            Runner.SendReliableDataToServer(chatKey, dataBuffer);
        }

        private void OnServerReceiveReliableMessage(NetworkRunner runner, PlayerRef pref, ReliableKey key, byte[] data)
        {
            if(key == chatKey)
            {
                if (!playerCellPosition.TryGetValue(pref, out int playerCell))
                    return;

                int count = Encoding.UTF8.GetCharCount(data);

                //deny if count exceeds a number
                
                //do some bad word filtering here

                Span<byte> tempAlloc = stackalloc byte[count + 4];

                int prefKey = pref.RawEncoded;

                unsafe
                {
                    fixed (byte* ptr = tempAlloc)
                    {
                        *(int*)ptr = prefKey;
                    }
                }

                Span<byte> dataSrc = new Span<byte>(data);

                dataSrc.CopyTo(tempAlloc.Slice(4));

                if(cellGroups.TryGetValue(playerCell, out var set))
				{
                    foreach(var p in set)
                    {
						runner.SendReliableDataToPlayer(p, chatKey, tempAlloc);
					}					
				}
            }
        }

        private void OnClientReceiveReliableMessage(NetworkRunner runner, PlayerRef pref, ReliableKey key, byte[] data)
        {
            if(key == chatKey)
            {
                int prefKey = 0;

                unsafe
                {
                    fixed(byte* ptr = data)
                    {
                        prefKey = *(int*)ptr;
                    }
                }

                PlayerRef player = PlayerRef.FromRaw(prefKey);

                string message = Encoding.UTF8.GetString(data, 4, data.Length - 4);

                Debug.Log($"Receive message from {player}: {message}");
            }
        }

		private void Update()
		{
            return;
			if(Keyboard.current != null)
            {
                if(Keyboard.current.spaceKey.wasReleasedThisFrame)
                {
                    Debug.Log("Space key!");
                    if(Runner != null && Runner.IsConnectedToServer)
					    SendChatMessage("Hello World!");
				}
            }
		}
	}
}
