using UnityEngine;

public class RobotController : MonoBehaviour {
    public Transform targetPosition;
    public Rigidbody rigid;
    public Transform headTrans;
    public Transform lookAt;
    public Transform cameraTrans;
    public float translationForce = 2f;
    public float rotateForce = 10f;
    public float headLookSpeed = 5f;
    public float randomLookSpeed = 0.1f;
    public float minDistFromLookTarget = 0.5f;
    public Vector2 horizontalFov = new Vector2(-70f, 70f);
    public Vector2 verticalFov = new Vector2(-25f, 45f);
    public Vector2 lookAtCameraDelay = new Vector2(4f, 8f);
    public float lookAtCameraDuration = 0.5f;

    private Quaternion defaultBodyRot;
    private Quaternion defaultHeadRot;
    private bool canSeeTarget;
    private bool lookingAtCamera;
    private Vector2 lookAtCameraTimeInterval;

    private void Awake() {
        defaultBodyRot = rigid.rotation;
        defaultHeadRot = headTrans.rotation;
        rigid.maxAngularVelocity = Mathf.Infinity;
    }
    
    private void FixedUpdate() {
        HandlePosition();
        HandleRotation();
        HandleHeadLook();
    }

    private void HandlePosition() {
        Vector3 targetPos = (targetPosition != null) ? targetPosition.position : rigid.position;

        // If too close to what it's looking at, then move away from it.
        if(canSeeTarget) {
            Vector3 dirToLookTarget = lookAt.position - rigid.position;
            float distToLookTarget = dirToLookTarget.magnitude;

            if(distToLookTarget > 0f && distToLookTarget < minDistFromLookTarget) {
                float distDiff = minDistFromLookTarget - distToLookTarget;  // Always positive.

                // Move target position until it is the minimum distance away.
                // Set vector length to distDiff.
                targetPos -= (dirToLookTarget / distToLookTarget) * distDiff;
            }
        }

        // Move towards the specified position.
        Vector3 offset = targetPos - rigid.position;

        if(offset.sqrMagnitude > Mathf.Epsilon)
            rigid.AddForce(offset * translationForce, ForceMode.Acceleration);
    }

    private void HandleRotation() {
        // Make robot always point in original direction using angular velocity.
        // Setting the rotation directly can lead to some unnatural collisions.
        Quaternion rotationOffset = defaultBodyRot * Quaternion.Inverse(rigid.rotation);

        // Convert rotation offset into a velocity vector.
        float angle;
        Vector3 axisVel;
        rotationOffset.ToAngleAxis(out angle, out axisVel);

        if(!float.IsInfinity(angle)) {
            // Angular velocity in radians, but ToAngleAxis outputs in degrees.
            rigid.angularVelocity = axisVel.normalized * angle * Mathf.Deg2Rad * rotateForce;
        }
    }

    private void HandleHeadLook() {
        Quaternion targetRot = defaultHeadRot;
        canSeeTarget = false;
        float t = Time.time;
        
        bool lookingAtCamera = (t > lookAtCameraTimeInterval.x && t < lookAtCameraTimeInterval.y);

        if(lookingAtCamera && cameraTrans != null) {
            targetRot = LookAtTarget(cameraTrans.position, out canSeeTarget);
        }
        else {
            if(lookAt != null) {
                targetRot = LookAtTarget(lookAt.position, out canSeeTarget);
            }

            // Get next time interval where robot will look at camera.
            if(t > lookAtCameraTimeInterval.y) {
                float nextLookAtCamStart = t + Random.Range(lookAtCameraDelay.x, lookAtCameraDelay.y);
                lookAtCameraTimeInterval = new Vector2(nextLookAtCamStart, nextLookAtCamStart + lookAtCameraDuration);
            }
        }

        if(!canSeeTarget) {
            // Look around randomly using perlin noise.
            float x = Mathf.PerlinNoise(t * randomLookSpeed, 0f);
            x = Mathf.LerpUnclamped(horizontalFov.x, horizontalFov.y, x);
            x += 180f;
            float y = Mathf.PerlinNoise(0f, t * randomLookSpeed);
            y = Mathf.LerpUnclamped(verticalFov.x, verticalFov.y, y);
            targetRot = rigid.rotation * Quaternion.Euler(y, x, 0f);    // Relative to body rotation.
        }

        // Interpolate head rotation.
        headTrans.rotation = Quaternion.Slerp(headTrans.rotation, targetRot, Time.deltaTime * headLookSpeed);
    }

    private Quaternion LookAtTarget(Vector3 pos, out bool canSeeTarget) {
        // Look towards the player's hand, but only if doing that won't break it's neck.
        Vector3 lookDir = pos - headTrans.position;
        Quaternion result = Quaternion.LookRotation(lookDir);

        // Get offset quaternion from the head and body rotations
        Quaternion rotationOffset = result * Quaternion.Inverse(rigid.rotation);
        Vector3 offsetEuler = rotationOffset.eulerAngles;

        // Convert quaternion's euler angle to simple angles relative to the direction the body is facing.
        float xAngle = GetSignedAngle(offsetEuler.y - 180f);
        float yAngle = -GetSignedAngle(offsetEuler.x);
        // Debug.Log(xAngle + "     " + yAngle);

        // The angles are within the specified FOV.
        canSeeTarget = (xAngle >= horizontalFov.x && xAngle <= horizontalFov.y && yAngle >= verticalFov.x && yAngle <= verticalFov.y);

        // Return the rotation quaternion.
        return result;
    }
    
    private float GetSignedAngle(float angle) {
        // 0 - 360  ->  -180 - 180
        if(angle > 180f) {
            return angle - 360f;
        }

        return angle;
    }
}