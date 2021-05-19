using UnityEngine;

namespace Minecraft
{
    public sealed class PlayerTerrainSensor : MonoBehaviour
    {
        private TerrainManager terrainManager = null;
        private IPlayerBody body = null;

        [SerializeField]
        private Rigidbody playerRigidbody = null;

        [SerializeField]
        [Range(1.0f, 10.0f)]
        private float blockClimbingSpeed = 6.0f;

        private float timeBuffer = 0.0f;

        private const float blockSensorAngleDegree = 70.0f;

        private void Start()
        {
            terrainManager = FindObjectOfType<TerrainManager>();
            body = GetComponent<IPlayerBody>();
        }

        private void Update()
        {
            timeBuffer -= Time.deltaTime;

            SetRigidbodyGravity();

            var ray = GetBlockSensor();
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * GetBlockSensorDistance());
            var pointOnTerrain = RaycastTerrainMesh(ray, GetBlockSensorDistance());

            if (pointOnTerrain != null)
            {
                if (IsMovingInto(pointOnTerrain))
                {
                    timeBuffer = 1.0f;

                    var position = transform.position;
                    transform.position = new Vector3(position.x, position.y + blockClimbingSpeed * Time.deltaTime, position.z);
                }
            }
        }

        private void SetRigidbodyGravity()
        {
            if (playerRigidbody != null)
            {
                var useGravity = !IsWorkingOnY;

                if (useGravity != playerRigidbody.useGravity)
                {
                    playerRigidbody.useGravity = useGravity;
                }
            }
        }

        private bool IsMovingInto(PointOnTerrainMesh pointOnTerrain)
        {
            return Vector3.Dot(body.GetMovementDirection(), pointOnTerrain.Point - body.GetFeetPosition()) > 0.0f;
        }

        private Ray GetBlockSensor()
        {
            var forwardDirection = body.GetForwardDirection();
            forwardDirection = Vector3.ProjectOnPlane(forwardDirection, Vector3.up);
            var direction = Quaternion.AngleAxis(-blockSensorAngleDegree, body.GetRightDirection(forwardDirection)) * forwardDirection;

            return new Ray(body.GetHeadPosition(), direction);
        }

        private float GetBlockSensorDistance()
        {
            return Vector3.Distance(body.GetHeadPosition(), body.GetFeetPosition()) / Mathf.Cos(Mathf.Deg2Rad * (90.0f - blockSensorAngleDegree)) - 0.05f;
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray ray, float maxDistance)
        {
            return terrainManager.RaycastTerrainMesh(ray, maxDistance);
        }

        public bool IsWorkingOnY { get => timeBuffer > 0.0f; }

        public float BlockClimbingSpeed { get => blockClimbingSpeed; }
    }
}