using EZCameraShake;
using System;
using UnityEngine;

namespace Minecraft
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IPlayerBody))]
    public sealed class Kamehameha : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private GameObject kamehamehaPrefab;

        [SerializeField]
        private GameObject explosionPrefab;

        [SerializeField]
        private Transform kamehamehaParent;

        [SerializeField]
        private GameObject firingLight;

        [SerializeField]
        private GameObject chargingLight;

        [SerializeField]
        private AudioManager audioManager;

        private AudioSource debrisSound;

        private float timeAcc = 0.0f;

        private bool isCharging = false;
        private bool isFiring = false;

        private TerrainManager terrainManager = null;
        private Func<Ray> getViewRayFunc = null;

        private int kamehamehaLayerMask;

        private int blocksCount = 0;
        private int foesCount = 0;

        private const float explosionRadius = 5.0f;
        private const float explosionForce = 5000.0f;

        private Rigidbody playerRigidbody;

        private IPlayerBody playerBody;

        public int BlocksCount { get => blocksCount; }

        public int FoesCount { get => foesCount; }

        private void Start()
        {
            kamehamehaLayerMask = LayerMask.GetMask("Receive Kamehameha");
            playerRigidbody = GetComponent<Rigidbody>();
            playerBody = GetComponent<IPlayerBody>();
        }

        private void Update()
        {
            if (gameState.IsPaused)
            {
                return;
            }

            timeAcc += Time.deltaTime;

            if (isCharging)
            {
                if (timeAcc >= 2.1f)
                {
                    Time.timeScale = 0.5f;
                    var terrainExplosion = FireOnTerrain();

                    if (terrainExplosion != null)
                    {
                        blocksCount += terrainExplosion.NumberOfBlocks;
                        Explode(terrainExplosion);

                        audioManager.CreateTemporaryAudioSourceAt("Explosion", terrainExplosion.PointOnTerrain.Point);
                    }

                    isCharging = false;
                    isFiring = true;

                    if (debrisSound != null)
                    {
                        Destroy(debrisSound.gameObject);
                        debrisSound = null;
                    }
                }
                else if (timeAcc >= 0.3f)
                {
                    if (!chargingLight.activeInHierarchy)
                    {
                        chargingLight.SetActive(true);
                    }
                }
            }

            if (isFiring)
            {
                if (timeAcc >= 3.74f)
                {
                    Time.timeScale = 1.0f;
                    isFiring = false;
                    firingLight.SetActive(false);
                    chargingLight.SetActive(false);
                }
                else
                {
                    if (!firingLight.activeInHierarchy)
                    {
                        firingLight.SetActive(true);
                    }

                    var terrainExplosion = FireOnTerrain();

                    if (terrainExplosion != null)
                    {
                        blocksCount += terrainExplosion.NumberOfBlocks;
                        ExplodeNearbyObjects(terrainExplosion, 0.5f);

                        UpdateDebrisSound(terrainExplosion.PointOnTerrain.Point);
                    }

                    FireOnObjects();
                }
            }
        }

        private void UpdateDebrisSound(Vector3 soundLocation)
        {
            if (debrisSound == null)
            {
                debrisSound = audioManager.CreateAudioSourceAt("Debris", soundLocation);
            }
            else
            {
                debrisSound.transform.position = soundLocation;
            }
        }

        private void FixedUpdate()
        {
            if (isFiring)
            {
                playerRigidbody.AddForce(-playerBody.GetForwardDirection() * 20.0f);
            }
        }

        public void ClearCounters()
        {
            blocksCount = 0;
            foesCount = 0;
        }

        public void Charge(TerrainManager terrainManager, Func<Ray> getViewRayFunc)
        {
            if (!isCharging && !isFiring)
            {
                CameraShaker.Instance.ShakeOnce(3.0f, 3.0f, 4.0f, 5.0f);

                timeAcc = 0.0f;
                isCharging = true;

                var kamehameha = Instantiate(kamehamehaPrefab, Vector3.zero, Quaternion.identity, kamehamehaParent);
                kamehameha.transform.localPosition = new Vector3(0.301f, -1.82f, 1.476f);
                kamehameha.transform.localRotation = Quaternion.Euler(0.0f, -2.4f, 0.0f);

                Destroy(kamehameha, 4.0f);

                this.terrainManager = terrainManager;
                this.getViewRayFunc = getViewRayFunc;

                audioManager.CreateTemporaryAudioSourceWithin("Kamehameha", transform);
            }
        }

        private TerrainExplosion FireOnTerrain()
        {
            TerrainExplosion terrainExplosion = null;

            var viewRay = getViewRayFunc();
            var pointOnTerrain = RaycastTerrainMesh(viewRay);         

            if (pointOnTerrain != null)
            {
                terrainExplosion = terrainManager.Explode(pointOnTerrain, explosionRadius);
            }

            return terrainExplosion;
        }

        private void Explode(TerrainExplosion terrainExplosion)
        {
            ExplodeNearbyObjects(terrainExplosion);
            PlayExplosion(explosionPrefab, terrainExplosion.PointOnTerrain.Point, new Vector3(2.0f, 2.0f, 2.0f));
        }

        private void FireOnObjects()
        {
            var viewRay = getViewRayFunc();

            var pointOnTerrain = RaycastTerrainMesh(viewRay);
            var maxRayDistance = pointOnTerrain != null ? Vector3.Distance(viewRay.origin, pointOnTerrain.Point) : 1000.0f;

            FireOnObjects(viewRay, maxRayDistance);
        }

        private PointOnTerrainMesh RaycastTerrainMesh(Ray viewRay)
        {
            return terrainManager.RaycastTerrainMesh(viewRay, float.MaxValue);
        }

        private void FireOnObjects(Ray viewRay, float maxDistance)
        {
            var colliders = Physics.OverlapCapsule(viewRay.origin, viewRay.GetPoint(maxDistance), explosionRadius * 0.2f, kamehamehaLayerMask);

            foreach (var c in colliders)
            {
                var explosionForceMultiplier = 0.2f;

                var foe = c.GetComponentInParent<FoeController>();

                if (foe != null)
                {
                    if (!foe.IsDead)
                    {
                        foe.Die();
                        ++foesCount;
                    }

                    explosionForceMultiplier = 0.04f;
                }

                var rb = FindRigidbody(c);

                if (rb != null)
                {
                    rb.AddForce(viewRay.direction * explosionForce * explosionForceMultiplier);
                }
            }
        }

        private void ExplodeNearbyObjects(TerrainExplosion explosion, float explosionMultiplier = 1.0f)
        {
            var usedExplosionForce = explosionForce * explosionMultiplier;
            var usedExplosionRadius = explosionRadius * 1.5f * explosionMultiplier;

            var colliders = Physics.OverlapSphere(explosion.ExplosionCenter, usedExplosionRadius);

            foreach (var c in colliders)
            {
                var foe = c.GetComponentInParent<FoeController>();

                if (foe != null && !foe.IsDead)
                {
                    foe.Die();
                    ++foesCount;
                }

                var rb = FindRigidbody(c);

                if (rb != null)
                {
                    rb.AddExplosionForce(usedExplosionForce, explosion.ExplosionCenter, usedExplosionRadius);
                }
            }

            foreach (var rb in explosion.CreatedBlocksBodies)
            {
                rb.AddExplosionForce(usedExplosionForce, explosion.ExplosionCenter, usedExplosionRadius);
            }
        }

        private static Rigidbody FindRigidbody(Collider c)
        {
            var rb = c.GetComponentInChildren<Rigidbody>();

            if (rb != null)
            {
                return rb;
            }

            return c.GetComponentInParent<Rigidbody>();
        }

        private static void PlayExplosion(GameObject prefab, Vector3 position, Vector3 scale)
        {
            var explosion = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            explosion.transform.localPosition = position;
            explosion.transform.localScale = scale;
            Destroy(explosion, 1.5f);
        }
    }
}