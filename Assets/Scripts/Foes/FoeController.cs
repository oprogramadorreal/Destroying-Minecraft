using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft
{
    public sealed class FoeController : MonoBehaviour, IPlayerBody
    {
        private IGameState gameState;
        private Animator animator;

        private IEnumerable<Collider> ragdollColliders;
        private IEnumerable<Rigidbody> ragdollBodies;

        private bool isDead = false;

        [SerializeField]
        private Transform target;

        [SerializeField]
        private PlayerTerrainSensor terrainSensor;

        [SerializeField]
        private Transform feet;

        [SerializeField]
        private Transform head;

        private AudioManager audioManager;
        private List<AudioSource> sounds = new List<AudioSource>();
        private List<AudioSource> dieSounds = new List<AudioSource>();

        private float speedFactor = 1.0f;
        private bool nightMode = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();

            ragdollColliders = GetComponentsInChildren<Collider>()
                .Where(c => c.gameObject != gameObject)
                .ToList();

            ragdollBodies = GetComponentsInChildren<Rigidbody>().ToList();

            SetRagdoll(false);
        }

        public void Revive(Transform newTarget, IGameState newGameState, AudioManager newAudioManager)
        {
            isDead = false;
            SetRagdoll(false);

            target = newTarget;
            gameState = newGameState;

            // force night mode verification on Update
            nightMode = !gameState.IsNight;

            audioManager = newAudioManager;
            CreateAudioSourcesIfNecessary();

            var soundIndex = Random.Range(0, sounds.Count);
            sounds[soundIndex].Play();
        }

        private void CreateAudioSourcesIfNecessary()
        {
            if (sounds.Count == 0)
            {
                sounds.Add(audioManager.CreateAudioSourceWithin("ZombieA", transform));
                sounds.Add(audioManager.CreateAudioSourceWithin("ZombieB", transform));
                sounds.Add(audioManager.CreateAudioSourceWithin("ZombieC", transform));
            }

            if (dieSounds.Count == 0)
            {
                dieSounds.Add(audioManager.CreateAudioSourceWithin("ZombieDieA", transform));
                dieSounds.Add(audioManager.CreateAudioSourceWithin("ZombieDieB", transform));
            }
        }

        private void SetRagdoll(bool enable)
        {
            foreach (var c in ragdollColliders)
            {
                c.isTrigger = !enable;
            }

            foreach (var b in ragdollBodies)
            {
                b.isKinematic = !enable;
            }

            animator.enabled = !enable;
        }

        private void Update()
        {
            if (CanUpdate())
            {
                UpdateNightMode();
                UpdateDirection();
                UpdatePosition();
            }
        }

        private bool CanUpdate()
        {
            return !isDead
                && target != null
                && gameState != null;
        }

        private void UpdateNightMode()
        {
            if (nightMode != gameState.IsNight)
            {
                speedFactor = gameState.IsNight ? 4.0f : 1.0f;
                animator.speed = speedFactor;
                nightMode = gameState.IsNight;
            }
        }

        private void UpdateDirection()
        {
            var desiredDirection = target.position - transform.position;
            desiredDirection.y = 0.0f;

            var rotation = Quaternion.LookRotation(desiredDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speedFactor * Time.deltaTime);
        }

        private void UpdatePosition()
        {
            var desiredPosition = transform.position + GetMovementDirection();
            var newPosition = Vector3.MoveTowards(transform.position, desiredPosition, speedFactor * Time.deltaTime);

            transform.position = new Vector3(newPosition.x, CalculateNewPositionY(), newPosition.z);
        }

        private float CalculateNewPositionY()
        {
            if (terrainSensor.IsWorkingOnY)
            {
                return transform.position.y; // PlayerTerrainSensor script is working on Y. Do not change it.
            }

            var pointOnTerrain = terrainSensor.RaycastTerrainMesh(new Ray(head.position, Vector3.down), float.MaxValue);

            if (pointOnTerrain == null)
            {
                return transform.position.y; // We haven't found the floor. Do not change Y.
            }

            return Mathf.Lerp(transform.position.y, pointOnTerrain.Point.y, terrainSensor.BlockClimbingSpeed * 2.0f * Time.deltaTime);
        }

        public bool IsDead { get => isDead; }

        public void Die()
        {
            SetRagdoll(true);
            isDead = true;

            if (dieSounds.Count > 0)
            {
                foreach (var sound in sounds)
                {
                    sound.Stop();
                }

                var soundIndex = Random.Range(0, dieSounds.Count);
                dieSounds[soundIndex].Play();
            }

            audioManager.CreateTemporaryAudioSource("Score");
        }

        public Vector3 GetMovementDirection()
        {
            if (isDead)
            {
                return Vector3.zero;
            }

            return GetForwardDirection();
        }

        public Vector3 GetForwardDirection()
        {
            return transform.TransformDirection(Vector3.forward);
        }

        Vector3 IPlayerBody.GetRightDirection(Vector3 forwardDirection)
        {
            return Vector3.Cross(forwardDirection, Vector3.up).normalized;
        }

        Vector3 IPlayerBody.GetHeadPosition()
        {
            return head.position;
        }

        Vector3 IPlayerBody.GetFeetPosition()
        {
            return feet.position;
        }
    }
}