using UnityEngine;

public class GroundStateUtil
{
    public IState Move(float speed, PlayerController playerController, IState groundState)
    {
        if (true/*CheckGrounded(playerController.transform, playerController.capsuleCollider, playerController.slopeLimit)*/)
        {
            if (playerController.localPlayerInputInfo.IsPressingForwards) playerController.MoveForward(speed);
            if (playerController.localPlayerInputInfo.IsPressingBack) playerController.MoveBackward(speed);
            if (playerController.localPlayerInputInfo.IsPressingRight) playerController.MoveRight(speed);
            if (playerController.localPlayerInputInfo.IsPressingLeft) playerController.MoveLeft(speed);
            if (playerController.localPlayerInputInfo.IsPressingJump) playerController.Jump(speed);

            return groundState;
        }
        else
        {
            return new MidairState().Update(playerController);
        }
    }

    public bool CheckGrounded(Transform transform, CapsuleCollider capsuleCollider, float slopeLimit)
    {
        float capsuleHeight = Mathf.Max(capsuleCollider.radius * 2f, capsuleCollider.height);
        Vector3 capsuleBottom = transform.TransformPoint(capsuleCollider.center - Vector3.up * capsuleHeight / 2f);
        float radius = transform.TransformVector(capsuleCollider.radius, 0f, 0f).magnitude;

        return GroundRaycast(capsuleBottom, radius, slopeLimit);
    }

    private bool GroundRaycast(Vector3 capsuleBottom, float radius, float slopeLimit)
    {
        Ray ray = new Ray(capsuleBottom + Vector3.up * .01f, -Vector3.up);

        if (Physics.Raycast(ray, out RaycastHit hit, radius * 5f))
        {
            float normalAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (normalAngle < slopeLimit)
            {
                float maxDist = radius / Mathf.Cos(Mathf.Deg2Rad * normalAngle) - radius + .02f;

                if (hit.distance < maxDist) return true;

                return false;
            }
        }

        return false;
    }
}