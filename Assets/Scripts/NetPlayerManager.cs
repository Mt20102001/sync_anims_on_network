using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CF.Game
{
    public struct PlayerId : INetworkStruct, System.IEquatable<PlayerId>
    {
        public ulong raw;

        public bool Equals(PlayerId other)
        {
            return other.raw == raw;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerId other && other.raw == raw;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + (int)(raw ^ (raw >> 32));
            return hash;
        }

        public static bool operator ==(PlayerId a, PlayerId b) { return a.raw == b.raw; }

        public static bool operator !=(PlayerId a, PlayerId b) { return a.raw != b.raw; }

        public static implicit operator ulong(PlayerId a) { return a.raw; }
        public static explicit operator PlayerId(ulong value) => new PlayerId { raw = value };

        public string ToX16String() => raw.ToString(@"X16");
    }

    public struct PlayerMinimapCoordinate : INetworkStruct
    {
        public Vector2 position;
        public byte direction;
    }

    public class NetPlayerManager : NetworkBehaviour
    {

        [Networked, Capacity(256)] public NetworkDictionary<PlayerId, PlayerMinimapCoordinate> PlayerCoordinates => default;

        public HashSet<NetPlayerObject> Players = new HashSet<NetPlayerObject>();

        [SerializeField] private NetPlayerObject playerObjectPrefab;

        public override void Spawned()
        {
            NetGame.PlayerManager = this;

            if (Runner.IsServer)
            {
                var events = Runner.GetBehaviour<NetworkEvents>();

                events.PlayerJoined.AddListener(OnPlayerJoined);
                events.PlayerLeft.AddListener(OnPlayerLeft);
            }
        }

        public void SpawnObjectForPlayer(PlayerRef player, Guid playerGuid)
        {
            if (Runner.TryGetPlayerObject(player, out _))
            {
                //player already has the object
                return;
            }

            var netPlayerObject = Runner.Spawn(playerObjectPrefab, inputAuthority: player, onBeforeSpawned: (runner, no) =>
            {
                var netPlayer = no.GetComponent<NetPlayerObject>();
                netPlayer.playerId = GenerateRandomPlayerId();
                netPlayer.playerGuid = playerGuid;
            });

            Players.Add(netPlayerObject);
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            //byte[] tokenStr = Runner.GetPlayerConnectionToken(player);

            //string token = Encoding.ASCII.GetString(tokenStr);

            //var payload = NakamaJWTValidation.ValidateNakamaToken(token, "Ugug2uTEkoKzhk7u5vmS6xOoRBtoiZ6KIcwbbSuQLTf0H4f5");

            //NetPlayerObject pObject = runner.Spawn(playerObjectPrefab, inputAuthority: player, onBeforeSpawned:(runner,no) =>
            //{
            //	no.GetComponent<NetPlayerObject>().playerId = GenerateRandomPlayerId();

            //	no.GetComponent<NetPlayerObject>().playerGuid = System.Guid.Parse(payload.uid);
            //});

            //pObject.name = player.ToString();

            //runner.SetPlayerAlwaysInterested(player, pObject.Object, true);

            if (Runner.IsServer)
            {
                SpawnObjectForPlayer(player, Guid.NewGuid());
            }

        }

        private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }

        private PlayerId GenerateRandomPlayerId()
        {
            ulong u = 0;

            for (int i = 0; i < 8; i++)
            {
                u |= ((ulong)(Random.Range(0, 256)) << (8 * i));
            }

            return (PlayerId)u;
        }
    }
}
