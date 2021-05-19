using EZCameraShake;
using System;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Based on this tutorial: https://youtu.be/NEUzB5vPYrE
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMovement : MonoBehaviour, IPlayerBody
    {
        [SerializeField]
        private GameState gameState;

        [SerializeField]
        private float walkSpeed = 2.0f;

        [SerializeField]
        private float playerJumpForce = 2000.0f;

        [SerializeField]
        private ForceMode appliedForceMode = ForceMode.Force;

        [SerializeField]
        private Transform playerHead;

        [SerializeField]
        private Transform headLight;

        [SerializeField]
        private AudioManager audioManager;

        private AudioSource walkingSound;

        [SerializeField]
        private Transform playerFeet;

        private bool isWalking = false;
        private bool needsToJump = false;
        private bool isOnTheFloor = false;

        private CameraShakeInstance cameraShake = null;

        private float currentSpeed;

        private float inputAxisX;
        private float inputAxisY;
        private float inputAxisZ;
        
        private Rigidbody playerRigidbody;

        private void Start()
        {
            playerRigidbody = GetComponent<Rigidbody>();
            walkingSound = audioManager.CreateAudioSourceWithin("Walking", transform);
        }

        private void Update()
        {
            if (gameState.IsPaused)
            {
                return;
            }

            UpdateAxes();

            currentSpeed = walkSpeed; // TODO: use runSpeed?
            isOnTheFloor = IsOnTheFloor();
            needsToJump = Input.GetButton("Jump") && isOnTheFloor;

            UpdateWalkingEffects();
        }

        private bool IsOnTheFloor()
        {
            var inOnTheFloor = false;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                var distanceToTheFloor = Vector3.Distance(playerFeet.position, hit.point);
                inOnTheFloor = distanceToTheFloor <= 0.2f;
            }

            return inOnTheFloor;
        }

        public void SetGodMode(bool enable)
        {
            playerRigidbody.isKinematic = enable;
        }

        private void UpdateAxes()
        {
            inputAxisX = Input.GetAxis("Horizontal");
            inputAxisY = Input.GetAxis("Up");
            inputAxisZ = Input.GetAxis("Vertical");
        }

        private void UpdateWalkingEffects()
        {
            if (!isOnTheFloor)
            {
                StopWalkingEffects(0.1f);
            }
            else
            {
                if (Math.Abs(inputAxisX) > 0.0f || Math.Abs(inputAxisZ) > 0.0f)
                {
                    if (!isWalking)
                    {
                        cameraShake = CameraShaker.Instance.StartShake(1.4f, 1.0f, 0.5f);
                        isWalking = true;
                        walkingSound.Play();
                    }
                }
                else
                {
                    StopWalkingEffects(2.0f);
                }
            }
        }

        private void StopWalkingEffects(float fadeOutTime)
        {
            if (isWalking)
            {
                if (cameraShake != null)
                {
                    cameraShake.StartFadeOut(fadeOutTime);
                }

                isWalking = false;
                walkingSound.Stop();
            }
        }

        private void FixedUpdate()
        {
            playerRigidbody.MovePosition(transform.position + Time.deltaTime * currentSpeed * CalculateMovementDirection());

            if (needsToJump)
            {
                Jump();
            }
        }

        private Vector3 CalculateMovementDirection()
        {
            var moveDirection = playerHead.TransformDirection(inputAxisX, inputAxisY, inputAxisZ);

            if (!gameState.IsGodMode)
            {
                moveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
            }

            return moveDirection.normalized;
        }

        private void Jump()
        {
            playerRigidbody.AddForce(playerJumpForce * playerRigidbody.mass * Time.deltaTime * Vector3.up, appliedForceMode);
        }

        Vector3 IPlayerBody.GetMovementDirection()
        {
            return CalculateMovementDirection();
        }

        Vector3 IPlayerBody.GetForwardDirection()
        {
            return playerHead.TransformDirection(Vector3.forward);
        }

        Vector3 IPlayerBody.GetRightDirection(Vector3 forwardDirection)
        {
            return Vector3.Cross(forwardDirection, Vector3.up).normalized;
        }

        Vector3 IPlayerBody.GetHeadPosition()
        {
            return headLight.position;
        }

        Vector3 IPlayerBody.GetFeetPosition()
        {
            return playerFeet.position;
        }
    }
}