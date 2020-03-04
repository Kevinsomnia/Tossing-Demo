using UnityEngine;

public class TossController : MonoBehaviour {
    public float dragForce = 25f;               // Higher values = snappier drag.
    public float dragDistance = 1f;             // Target distance the object should be from the camera when dragging.
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

            if(Physics.Raycast(rayToCursor, out hit, 25f)) {
                if(currentlyHolding != null) {
                    // Ensure previously held object is released before holding new object.
                    ReleaseHeldObject();
                }

                HoldObject(hit.collider.GetComponent<TossableObject>());
            }
        }
        else {
            if(Input.GetMouseButtonUp(0) && currentlyHolding != null) {
                // Released left mouse button.
                ReleaseHeldObject();
            }
        }
    }
    
    private void FixedUpdate() {
        if(currentlyHolding != null) {
            // Get the target position of dragged object.
            Vector3 targetPoint = GetPointTowardsCursor(dragDistance);

            // Check for abrupt deceleration in velocity while dragging.
            // Usually means there is something blocking it's path (if high enough).
            float decelAmount = prevHeldVelocity - currentlyHolding.cachedRigid.velocity.magnitude;

            if(decelAmount > dragTensionLimit) {
                ReleaseHeldObject();
            }
            else {
                // Instantly update velocity towards target point.
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
        float torqueAmount = 0.0005f + (currentlyHolding.cachedRigid.velocity.magnitude * 0.002f);
        currentlyHolding.cachedRigid.AddTorque(Random.onUnitSphere * torqueAmount, ForceMode.Impulse);

        // Send toss event.
        currentlyHolding.OnTossed();
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