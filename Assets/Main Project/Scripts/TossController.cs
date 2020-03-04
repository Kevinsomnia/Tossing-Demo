using UnityEngine;

public class TossController : MonoBehaviour {
    public float dragForce = 25f;               // Higher values = snappier drag.
    public float defaultDragDistance = 1f;             // Target distance the object should be from the camera when dragging.
    public float minDragDistance = 0.4f;
    public float maxDragDistance = 1.5f;
    public float dragDistanceStep = 0.05f;
    public float tossForce = 4f;                // Release toss force.
    public float dragTensionLimit = 200f;       // Minimum threshold before auto-releasing object due to something blocking it's path.
    public Transform handRig;
    public GameObject openHand;
    public GameObject closedHand;

    private Transform cachedTrans;
    private Camera cachedCam;
    private TossableObject currentlyHolding;
    private float prevHeldVelocity;
    private float rigidOrigAngDrag;
    private float curDragDistance;
    private float visualHandDist;

    private void Awake() {
        cachedTrans = transform;
        cachedCam = GetComponent<Camera>();
        currentlyHolding = null;
        prevHeldVelocity = 0f;
        curDragDistance = defaultDragDistance;
    }
    
    private void Update() {
        // Control the hand rig positioning and try to avoid clipping using the raycast.
        UpdateHandVisual();

        // Controlling hand distance by scrolling.
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if(scrollWheel < -Mathf.Epsilon) {
            curDragDistance -= dragDistanceStep;
            curDragDistance = Mathf.Max(minDragDistance, curDragDistance);
        }
        else if(scrollWheel > Mathf.Epsilon) {
            curDragDistance += dragDistanceStep;
            curDragDistance = Mathf.Min(curDragDistance, maxDragDistance);
        }

        // Handle dragging and releasing/tossing an object.
        if(Input.GetMouseButtonDown(0)) {
            Ray rayToCursor = cachedCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Buffer distance to allow grabbing objects further from hand.
            float rayDistance = curDragDistance * 2f;

            if(Physics.Raycast(rayToCursor, out hit, rayDistance)) {
                if(currentlyHolding != null) {
                    // Ensure previously held object is released before holding new object.
                    ReleaseHeldObject();
                }

                HoldObject(hit.collider.GetComponent<TossableObject>());
            }
        }
        else {
            if(Input.GetMouseButtonUp(0) && currentlyHolding != null) {
                ReleaseHeldObject();
            }
        }
    }
    
    private void FixedUpdate() {
        if(currentlyHolding != null) {
            // Get the target position of dragged object.
            Vector3 targetPoint = GetPointTowardsCursor(curDragDistance);

            // Check for abrupt deceleration in velocity while dragging.
            // Usually means there is something blocking it's path (if high enough).
            float decelAmount = prevHeldVelocity - currentlyHolding.cachedRigid.velocity.magnitude;

            if(decelAmount > dragTensionLimit) {
                ReleaseHeldObject();
            }
            else {
                // Don't set rigidbody position since it can clip through objects. Instead, we set the velocity towards
                // the target point.
                currentlyHolding.cachedRigid.velocity = (targetPoint - currentlyHolding.cachedRigid.position) * dragForce;
                prevHeldVelocity = currentlyHolding.cachedRigid.velocity.magnitude;
            }
        }
    }

    private void HoldObject(TossableObject obj) {
        if(obj == null) {
            ReleaseHeldObject();
            return;
        }

        currentlyHolding = obj;
        prevHeldVelocity = obj.cachedRigid.velocity.magnitude;

        // Updating hand distance to where the visual hand is.
        curDragDistance = visualHandDist;
        curDragDistance = Mathf.Clamp(curDragDistance, minDragDistance, maxDragDistance);

        // Send grab event.
        currentlyHolding.OnGrabbed();
        
        // "Freeze" rotation while held. Restore angular drag upon releasing.
        rigidOrigAngDrag = obj.cachedRigid.angularDrag;
        obj.cachedRigid.angularDrag = 10f;
    }

    private void ReleaseHeldObject() {
        if(currentlyHolding == null)
            return;
        
        // Restore angular drag and add some random torque. More torque when released with more force.
        currentlyHolding.cachedRigid.angularDrag = rigidOrigAngDrag;
        float torqueAmount = 0.0005f + (currentlyHolding.cachedRigid.velocity.magnitude * 0.001f);
        currentlyHolding.cachedRigid.AddTorque(Random.onUnitSphere * torqueAmount, ForceMode.Impulse);

        // Send toss event.
        currentlyHolding.OnTossed();
        currentlyHolding = null;
    }

    private Ray GetRayTowardsCursor() {
        return cachedCam.ScreenPointToRay(Input.mousePosition);
    }
    
    private Vector3 GetPointTowardsCursor(float distance) {
        Ray rayToCursor = GetRayTowardsCursor();
        return cachedTrans.position + (rayToCursor.direction * distance);
    }

    private void UpdateHandVisual() {
        const float VISUAL_HAND_OFFSET_Z = -0.1f;

        // Position the hand a little behind the drag distance.
        float handDist = curDragDistance + VISUAL_HAND_OFFSET_Z;
        Ray rayToCursor = GetRayTowardsCursor();
        Vector3 handPos = GetPointTowardsCursor(handDist);

        if(currentlyHolding == null) {
            RaycastHit hit;

            if(Physics.Raycast(rayToCursor, out hit, handDist)) {
                float distDelta = hit.distance - handDist; // Always negative.
                handPos += rayToCursor.direction * distDelta; // Shift hand position to hit point.
            }
        }
        
        Quaternion handRot = Quaternion.LookRotation(handPos - cachedTrans.position);
        handRig.SetPositionAndRotation(handPos, handRot);

        // Update visual hand distance from camera, accounting for the visual offset.
        visualHandDist = (handPos - cachedTrans.position).magnitude - VISUAL_HAND_OFFSET_Z;

        // Update finger.
        bool showClosedHand = (currentlyHolding != null);
        closedHand.SetActive(showClosedHand);
        openHand.SetActive(!showClosedHand);
    }
}