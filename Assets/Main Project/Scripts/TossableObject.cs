using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TossableObject : MonoBehaviour {
    // Global constants for all tossables.
    private const float DRAG_COEFFICIENT = -0.05f;
    private const float IDLE_VELOCITY_THRESHOLD = 0.02f * 0.02f;
    private const float TOSS_DAMPENING = 0.5f;
    private const float BOOMERANG_DELAY = 0.5f;                         // To prevent from boomeranging immediately.
    private const float BOOMERANG_VELOCITY_THRESHOLD = 0.2f * 0.2f;     // Velocity of object before starting to boomerang.
    private const float BOOMERANG_FORCE = 0.9f;
    private const float BOOMERANG_MAX_VELOCITY = 4f * 4f;               // Stop boomerang from coming back too fast.
    private const float BOOMERANG_PROX_THRESHOLD = 0.15f * 0.15f;       // Stop boomerang when within 15 cm of tossed pos.

    public enum State { Idle, Boomerang_Wait, Boomeranging, Grabbed };

    public Rigidbody cachedRigid { get; private set; }

    private State status;
    private Vector3 tossedPos;
    private float lastTossedTime;

    private void Awake() {
        cachedRigid = GetComponent<Rigidbody>();
        status = State.Idle;
    }

    private void FixedUpdate() {
        if(status != State.Grabbed && status != State.Boomeranging) {
            ApplyDrag();
        }
        
        if(status == State.Boomerang_Wait) {
            // Wait for object to become slow enough before coming back.
            if(Time.time - lastTossedTime >= BOOMERANG_DELAY && cachedRigid.velocity.sqrMagnitude < BOOMERANG_VELOCITY_THRESHOLD) {
                status = State.Boomeranging;
            }
        }
        else if(status == State.Boomeranging) {
            // Apply boomerang force.
            ApplyBoomerangForce();
        }
    }

    public void OnGrabbed() {
        status = State.Grabbed;
    }

    public void OnTossed() {
        status = State.Boomerang_Wait;
        tossedPos = cachedRigid.position;
        lastTossedTime = Time.time;
        cachedRigid.velocity *= TOSS_DAMPENING;
    }

    private void ApplyDrag() {
        if(cachedRigid.velocity.sqrMagnitude > IDLE_VELOCITY_THRESHOLD) {
            // Slow down object gradually when above the idle threshold.
            cachedRigid.AddForce(cachedRigid.velocity * DRAG_COEFFICIENT, ForceMode.VelocityChange);
        }
    }

    private void ApplyBoomerangForce() {
        Vector3 dirToTossedPos = tossedPos - cachedRigid.position;

        if(dirToTossedPos.sqrMagnitude > BOOMERANG_PROX_THRESHOLD) {
            if(cachedRigid.velocity.sqrMagnitude < BOOMERANG_MAX_VELOCITY) {
                cachedRigid.AddForce(dirToTossedPos * BOOMERANG_FORCE, ForceMode.Acceleration);
            }
        }
        else {
            status = State.Idle;
        }
    }
}