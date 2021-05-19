using UnityEngine;

namespace Minecraft
{
    public interface IPlayerBody
    {
        Vector3 GetMovementDirection();

        Vector3 GetForwardDirection();

        Vector3 GetRightDirection(Vector3 forwardDirection);

        Vector3 GetHeadPosition();

        Vector3 GetFeetPosition();
    }
}