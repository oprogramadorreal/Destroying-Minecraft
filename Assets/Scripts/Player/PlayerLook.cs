using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Based on this tutorial: https://youtu.be/_QajrabyTJc
    /// </summary>
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField]
        private float mouseSensitivity = 10.0f;

        [SerializeField]
        private GameState gameState;

        private float xRotation = 0.0f;

        private Vector3? killerPosition = null;

        private void Update()
        {
            if (killerPosition.HasValue)
            {
                var direction = (killerPosition.Value - transform.position).normalized;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), Time.unscaledDeltaTime * 20.0f);
            }

            if (gameState.IsPaused)
            {
                return;
            }

            if (!Cursor.visible || Input.GetMouseButton(1))
            {
                // Not using Time.deltaTime, as suggested in:
                // https://answers.unity.com/questions/490687/when-to-use-timedeltatime.html
                // Using timeScale^3 instead.

                var rotationSpeed = mouseSensitivity * Mathf.Pow(Time.timeScale, 3.0f);

                var rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotationSpeed;

                xRotation += Input.GetAxis("Mouse Y") * rotationSpeed;
                xRotation = Mathf.Clamp(xRotation, -90, 90);

                transform.localEulerAngles = new Vector3(-xRotation, rotationX, 0);
            }
        }

        public void SetKillerPosition(Vector3 newKillerPosition)
        {
            killerPosition = newKillerPosition;
        }
    }
}
