using UnityEngine;

public class TossController : MonoBehaviour {
    public float maxGrabDistance = 5f;          // Can grab objects within this distance.
    public float dragForce = 25f;               // Higher values = snappier drag.
    public float dragDistance = 1f;             // Target distance the object should be from the camera when dragging.
    public float tossForce = 4f;                // Release toss force.
    public float dragTensionLimit = 200f;       // Minimum threshold before auto-releasing object due to something blocking it's path.
    public Transform handRig;
    public GameObject openHand;
    public GameObject closedHand;

    private Transform cachedTrans;
    private Camera cachedCam;
    private Rigidbody currentlyHolding;
    private float prevHeldVelocity;
    private float rigidOrigAngDrag;

    private void Awake() {
        cachedTrans = transform;
        cachedCam = GetComponent<Camera>();
        currentlyHolding = null;
        prevHeldVelocity = 0f;
    }
    
    private void Update() {
        UpdateHandVisual();

        if(Input.GetMouseButtonDown(0)) {
            // Start dragging object on left click.
            Ray rayToCursor = cachedCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(rayToCursor, out hit, maxGrabDistance)) {
                if(currentlyHolding != null) {
                    // Ensure previously held object is released.
                    ReleaseHeldObject(false);
                }

                HoldObject(hit.rigidbody);
            }
        }
        else {
            if(Input.GetMouseButtonUp(0) && currentlyHolding != null) {
                // Released left mouse button.
                ReleaseHeldObject(true);
            }
        }
    }
    
    private void FixedUpdate() {
        if(currentlyHolding != null) {
            // Get the target position of dragged object.
            Vector3 targetPoint = GetPointTowardsCursor(dragDistance);

            // Check for abrupt deceleration in velocity while dragging.
            // Usually means there is something blocking it's path (if high enough).
            float decelAmount = prevHeldVelocity - currentlyHolding.velocity.magnitude;

            if(prevHeldVelocity - currentlyHolding.velocity.magnitude > dragTensionLimit) {
                ReleaseHeldObject(true);
            }
            else {
                // Instantly update velocity towards target point.
                currentlyHolding.velocity = (targetPoint - currentlyHolding.position) * dragForce;
                prevHeldVelocity = currentlyHolding.velocity.magnitude;
            }
        }
    }

    private void HoldObject(Rigidbody rigid) {
        if(rigid == null) {
            ReleaseHeldObject(false);
            return;
        }

        currentlyHolding = rigid;
        prevHeldVelocity = currentlyHolding.velocity.magnitude;

        // "Freeze" rotation while held. Restore angular drag upon releasing.
        rigidOrigAngDrag = currentlyHolding.angularDrag;
        currentlyHolding.angularDrag = 10f;
    }

    private void ReleaseHeldObject(bool dampenVelocity) {
        if(currentlyHolding == null)
            return;
        
        if(dampenVelocity) {
            // Dampen velocity before releasing, since dragging the object relies on a lot of force.
            currentlyHolding.velocity *= (tossForce / dragForce);
        }

        // Restore angular drag and add some random torque. More torque when released with more force.
        currentlyHolding.angularDrag = rigidOrigAngDrag;
        float torqueAmount = 0.0005f + (currentlyHolding.velocity.magnitude * 0.002f);
        currentlyHolding.AddTorque(Random.onUnitSphere * torqueAmount, ForceMode.Impulse);
        currentlyHolding = null;
    }
    
    private Vector3 GetPointTowardsCursor(float distance) {
        Ray rayToCursor = cachedCam.ScreenPointToRay(Input.mousePosition);
        return cachedTrans.position + (rayToCursor.direction * distance);
    }

    private void UpdateHandVisual() {
        // Position hand where drag distance is.
        Vector3 handPos = GetPointTowardsCursor(dragDistance - 0.1f);
        Quaternion handRot = Quaternion.LookRotation(handPos - cachedTrans.position);
        handRig.SetPositionAndRotation(handPos, handRot);

        // Update finger.
        bool showClosedHand = (currentlyHolding != null);
        closedHand.SetActive(showClosedHand);
        openHand.SetActive(!showClosedHand);
    }
}