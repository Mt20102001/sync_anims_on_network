namespace ExpertMovement
{
    using Fusion;
    using UnityEngine;

    /// <summary>
    /// Component maintaining player abilities - spawning, parenting, ...
    /// </summary>
    [DefaultExecutionOrder(-15)]
    public sealed class ExpertPlayerAbilities : NetworkBehaviour
    {
        // CONSTANTS

        private const int MAX_ABILITIES = 8;

        // PRIVATE MEMBERS


        [Networked]
        [Capacity(MAX_ABILITIES)]
        private NetworkArray<NetworkObject> _networkedAbilities { get; }

        private NetworkObject[] _localAbilities = new NetworkObject[MAX_ABILITIES];

        // PUBLIC METHODS


        public enum POS
        {
            TOP,
            MIDDLE,
            BOTTOM,
        }


        private RaycastHit[] hits;
        public Vector3 GetBlockPos(POS pos)
        {
            Vector3 checkPoint = default;
            switch (pos)
            {
                case POS.TOP:
                    checkPoint = TopPointCheck;
                    break;

                case POS.MIDDLE:
                    checkPoint = MidPointCheck;
                    break;

                case POS.BOTTOM:
                    checkPoint = BotPointCheck;
                    break;
            }
            return checkPoint;
        }


        public bool CheckBlocks(Vector3 checkPoint, Vector3 direction, float distanceCheck, float thinThreshold, LayerMask geometryMask, out bool isThinBlock, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            isThinBlock = false;
            hitPoint = default;
            hitNormal = default;
            var result = CheckObstacle(checkPoint, direction, distanceCheck, thinThreshold, geometryMask, ref hits, out int hitIndex);
            isThinBlock = result.isThin;
            bool haveBlock = result.hasObstacle;
            if (haveBlock)
            {
                var hit = hits[hitIndex];
                hitPoint = hit.point;
                hitNormal = hit.normal;
                //Debug.Log($"hitted {hit.collider.name}");
            }


            //Debug.DrawLine(checkPoint, checkPoint + direction * distanceCheck, result.isThin ? Color.green : Color.red, 3f);

            return haveBlock;
        }


        struct ObstacleCheckResult
        {
            public bool hasObstacle;
            public bool isThin;
            public float thickness;
        }


        private ObstacleCheckResult CheckObstacle(Vector3 origin, Vector3 dir, float distance, float thinThreshold, LayerMask geometryMask, ref RaycastHit[] results, out int hitIndex)
        {
            ObstacleCheckResult result = new ObstacleCheckResult();
            hitIndex = -1;
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                dir,
                results,
                distance,
                geometryMask,
                QueryTriggerInteraction.Ignore
            );

            //Debug.DrawLine(origin, origin + dir * distance, Color.blue, 3f);

            if (hitCount == 0)
                return result;


            var targetBlock = results[0];
            //if (!targetBlock.collider.gameObject.isStatic || !targetBlock.collider.transform.parent.gameObject.isStatic)
            //    return result;

            result.hasObstacle = true;
            Vector3 fPoint = targetBlock.point;
            Vector3 sPoint = fPoint + dir * thinThreshold;

            if (hitCount >= 2)
            {
                sPoint = results[1].point;
            }

            float thickness = thinThreshold;
            if (Physics.Raycast(sPoint, -dir, out var rayHit, Vector3.Distance(fPoint, sPoint), geometryMask, QueryTriggerInteraction.Ignore))
            {
                thickness = Vector3.Distance(rayHit.point, fPoint);
            }

            result.isThin = thickness < thinThreshold;
            hitIndex = 0;

            return result;
        }


        public bool TryGetAbility<T>(out T ability) where T : class
        {
            // This method iterates over LOCAL array of abilities to ensure parenting is already done and the ability is at correct position in hierarchy.
            for (int i = 0, count = _localAbilities.Length; i < count; ++i)
            {
                NetworkObject localAbility = _localAbilities[i];
                if (localAbility != null && localAbility.TryGetComponent<T>(out T component) == true)
                {
                    ability = component;
                    return true;
                }
            }

            ability = default;
            return default;
        }

        public NetworkObject AddAbility(NetworkObject abilityPrefab)
        {
            return AddAbility(abilityPrefab, true);
        }

        public bool RemoveAbility(NetworkObject ability)
        {
            if (HasStateAuthority == false)
                return default;

            for (int i = 0; i < _localAbilities.Length; ++i)
            {
                if (_localAbilities[i] == ability)
                {
                    _localAbilities[i] = default;
                    break;
                }
            }

            for (int i = 0; i < _networkedAbilities.Length; ++i)
            {
                NetworkObject networkedAbility = _networkedAbilities.Get(i);
                if (networkedAbility == ability)
                {
                    _networkedAbilities.Set(i, default);
                    networkedAbility.transform.SetParent(null);
                    Runner.Despawn(networkedAbility);
                    Debug.Log("Remove Ability");
                    return true;
                }
            }

            return default;
        }

        public void AddAbilities(NetworkObject[] abilityPrefabs)
        {
            bool synchronizeLocalAbilities = false;

            for (int i = 0; i < abilityPrefabs.Length; ++i)
            {
                synchronizeLocalAbilities |= AddAbility(abilityPrefabs[i], false) != null;
            }

            if (synchronizeLocalAbilities == true)
            {
                SynchronizeLocalAbilities();
            }
        }

        public void RemoveAbilities()
        {
            if (HasStateAuthority == false)
                return;

            ClearLocalAbilities();

            for (int i = 0, count = _networkedAbilities.Length; i < count; ++i)
            {
                NetworkObject networkedAbility = _networkedAbilities[i];
                if (networkedAbility != null)
                {
                    Runner.Despawn(networkedAbility);
                }

                _networkedAbilities.Set(i, null);
                Debug.Log("Remove Ability");
            }
        }

        // NetworkBehaviour INTERFACE

        public override void Spawned()
        {
            hits = new RaycastHit[5];
            SynchronizeLocalAbilities();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ClearLocalAbilities();
            RemoveAbilities();
        }

        public override void FixedUpdateNetwork()
        {
            SynchronizeLocalAbilities();
            UpdateCheckPoints();
        }

        // MonoBehaviour INTERFACE

        private void Update()
        {
            if (IsProxy == false)
                return;

            // Proxy don't execute FixedUpdateNetwork(), we have to synchronize abilities from Update().
            SynchronizeLocalAbilities();
        }

        // PRIVATE METHODS

        public Vector3 TopPointCheck { get; private set; }
        public Vector3 MidPointCheck { get; private set; }
        public Vector3 BotPointCheck { get; private set; }
        private void UpdateCheckPoints()
        {
            BotPointCheck = transform.position + Vector3.up * 0.05f;
            TopPointCheck = transform.position + Vector3.up * 0.6f;
            MidPointCheck = (BotPointCheck + TopPointCheck) * 0.5f;
        }


        private NetworkObject AddAbility(NetworkObject abilityPrefab, bool synchronizeLocalAbilities)
        {
            if (abilityPrefab == null)
                return default;

            if (HasStateAuthority == false)
                return default;

            for (int i = 0; i < _networkedAbilities.Length; ++i)
            {
                NetworkObject networkedAbility = _networkedAbilities.Get(i);
                if (networkedAbility != null)
                    continue;

                networkedAbility = Runner.Spawn(abilityPrefab, transform.position, null, Object.InputAuthority, SetParentTransform);
                _networkedAbilities.Set(i, networkedAbility);

                //Debug.Log($"_networkedAbilities at {i} = {_networkedAbilities[i]}");

                if (synchronizeLocalAbilities == true)
                {
                    SynchronizeLocalAbilities();
                }

                return networkedAbility;
            }

            return default;
        }


        [ContextMenu("Log _networkedAbilities")]
        private void LogNetworkedAbilities()
        {
            for (int i = 0; i < _networkedAbilities.Length; ++i)
            {
                Debug.Log($"_networkedAbilities at {i} = {_networkedAbilities[i]}");
            }
        }


        private void SynchronizeLocalAbilities()
        {
            // This method synchronizes networked list of abilities with a local list.
            // This approach is robust and ensures correct initialization on all peers - server / host / client in all modes.

            for (int i = 0, count = _networkedAbilities.Length; i < count; ++i)
            {
                NetworkObject networkedAbility = _networkedAbilities[i];
                NetworkObject localAbility = _localAbilities[i];

                if (localAbility == networkedAbility)
                    continue;

                if (localAbility != null)
                {
                    localAbility.transform.SetParent(null);
                }

                localAbility = networkedAbility;

                localAbility.transform.SetParent(transform);
                localAbility.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _localAbilities[i] = localAbility;
            }
        }

        private void ClearLocalAbilities()
        {
            for (int i = 0, count = _localAbilities.Length; i < count; ++i)
            {
                NetworkObject localAbility = _localAbilities[i];
                if (localAbility != null && localAbility.IsValid == true)
                {
                    localAbility.transform.SetParent(null);
                }

                _localAbilities[i] = null;
            }
        }

        private void SetParentTransform(NetworkRunner runner, NetworkObject networkObject)
        {
            networkObject.transform.SetParent(transform);
        }
    }
}
