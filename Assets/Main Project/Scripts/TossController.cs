using UnityEngine;

public class TossController : MonoBehaviour {
    public float throwForce = 1f;

    private Transform cachedTrans;
    private Camera cachedCam;
    private Rigidbody currentlyHolding;
    private float holdDist;

    private void Awake() {
        cachedTrans = transform;
        cachedCam = GetComponent<Camera>();
        currentlyHolding = null;
    }

    private void Update() {
        if(Input.GetMouseButtonDown(0)) {
            // Start dragging object on left click.
            Ray rayToCursor = cachedCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(rayToCursor, out hit, 50f)) {
                if(currentlyHolding != null) {
                    // Ensure previously held object is released.
                    ReleaseAndToss();
                }

                currentlyHolding = hit.rigidbody;

                if(currentlyHolding != null) {
                    // This is a physics object and not just a static wall.
                    holdDist = Vector3.Distance(cachedTrans.position, currentlyHolding.position);
                    Debug.Log("Holding " + currentlyHolding.name + " at distance from camera: " + holdDist);
                }
            }
        }
        else {
            if(currentlyHolding != null) {
                if(Input.GetMouseButtonUp(0)) {
                    // Released left mouse button.
                    ReleaseAndToss();
                }
                else if(Input.GetMouseButtonDown(1)) {
                    // Throw away from camera upon right click.
                    Vector3 dirToObject = (currentlyHolding.position - cachedTrans.position).normalized;
                    currentlyHolding.AddForce(dirToObject * throwForce, ForceMode.Impulse);
                    currentlyHolding.AddTorque(Random.insideUnitSphere * 0.001f * throwForce, ForceMode.Impulse);
                    ReleaseAndToss();
                }
            }
        }
    }

    private void FixedUpdate() {
        // Handle dragging in FixedUpdate.
        if(currentlyHolding != null) {
            currentlyHolding.position = cachedTrans.position + (GetDirectionTowardsMouseCursor() * holdDist);
            currentlyHolding.rotation = cachedTrans.rotation;
        }
    }

    private void ReleaseAndToss() {
        //currentlyHolding.AddForce(Vector3.up, ForceMode.Impulse);
        currentlyHolding = null;
    }
    
    private Vector3 GetDirectionTowardsMouseCursor() {
        Ray rayToCursor = cachedCam.ScreenPointToRay(Input.mousePosition);
        return rayToCursor.direction;
    }
}