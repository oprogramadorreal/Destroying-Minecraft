using UnityEngine;

namespace Minecraft
{
    [RequireComponent(typeof(ObjectsPool))]
    public sealed class FoesManager : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private AudioManager audioManager;

        [SerializeField]
        private Transform player;

        [SerializeField]
        private float minSpawnDistance = 10.0f;

        [SerializeField]
        private float maxSpawnDistance = 20.0f;

        [SerializeField]
        private float minSpawnTimeInSeconds = 2.0f;

        [SerializeField]
        private float maxSpawnTimeInSeconds = 4.0f;

        private ObjectsPool foesPool;

        private float timeToSpawnNext = 0.0f;
        private bool isSpawning = false;

        private void Start()
        {
            foesPool = GetComponent<ObjectsPool>();
        }

        private void Update()
        {
            if (gameState.IsPaused)
            {
                return;
            }

            if (isSpawning)
            {
                timeToSpawnNext -= Time.deltaTime;

                if (timeToSpawnNext <= 0.0f)
                {
                    Spawn();
                    timeToSpawnNext = Random.Range(minSpawnTimeInSeconds, maxSpawnTimeInSeconds);
                }
            }
        }

        private void Spawn()
        {
            var newFoe = foesPool.Instantiate(GetFoeRandomPosition());
            var newFoeController = newFoe.GetComponent<FoeController>();

            newFoeController.Revive(player, gameState, audioManager);
        }

        private Vector3 GetFoeRandomPosition()
        {
            var point2d = GetRandom2dPointInRangeDistance(minSpawnDistance, maxSpawnDistance);
            const float spawnHeight = 100.0f;

            return player.position + new Vector3(point2d.x, spawnHeight, point2d.y);
        }

        private static Vector2 GetRandom2dPointInRangeDistance(float minDistance, float maxDistance)
        {
            var v = GetRandom2dPointInsideUnitCircle();
            var vNorm = v.normalized;
            return (vNorm * minDistance) + vNorm * (maxDistance - minDistance);
        }

        private static Vector2 GetRandom2dPointInsideUnitCircle()
        {
            Vector2 result;

            do
            {
                result = Random.insideUnitCircle;
            }
            while (result.sqrMagnitude <= Mathf.Epsilon);

            return result;
        }

        public void StartSpawning()
        {
            timeToSpawnNext = 0.0f;
            isSpawning = true;
        }
    }
}