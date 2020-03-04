using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TossableObject : MonoBehaviour {
    // Global constants for all tossables.
    private const float DRAG_COEFFICIENT = -0.05f;
    private const float IDLE_VELOCITY_THRESHOLD = 0.02f * 0.02f;
    private const float TOSS_FORCE_DAMPENING = 0.5f;
    private const float BOOMERANG_DELAY = 0.5f;                         // To prevent from boomeranging immediately.
    private const float BOOMERANG_TRIGGER_THRESHOLD = 0.4f * 0.4f;      // Minimum toss velocity in order to start boomeranging.
    private const float BOOMERANG_RETURN_THRESHOLD = 0.2f * 0.2f;       // Velocity of object before boomeranging back.
    private const float BOOMERANG_FORCE = 0.9f;
    private const float BOOMERANG_VELOCITY_LIMIT = 3f;                  // Prevent boomerang from coming back too fast.

    public static bool boomerangEnabled = true;

    public enum State { Idle, Boomerang_Wait, Boomeranging, Grabbed };

    public Rigidbody cachedRigid { get; private set; }

    private State status;
    private Vector3 tossedPos;
    private Vector3 tossedVelocity;
    private float lastTossedTime;

    private void Awake() {
        cachedRigid = GetComponent<Rigidbody>();
        status = State.Idle;
    }

    private void FixedUpdate() {
        if(status != State.Grabbed && status != State.Boomeranging) {
            ApplyDrag();
        }

        // Boomerang force is too weak to fight against gravity (if something other than no gravity is selected).
        cachedRigid.useGravity = (status != State.Boomerang_Wait && status != State.Boomeranging);

        if(status == State.Boomerang_Wait) {
            // Wait for object to become slow enough before coming back.
            if(Time.time - lastTossedTime >= BOOMERANG_DELAY && cachedRigid.velocity.sqrMagnitude < BOOMERANG_RETURN_THRESHOLD) {
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
        tossedPos = cachedRigid.position;
        tossedVelocity = cachedRigid.velocity;
        lastTossedTime = Time.time;

        // Check that boomeranging is enabled and tossed hard enough.
        bool shouldBoomerang = (boomerangEnabled && tossedVelocity.sqrMagnitude >= BOOMERANG_TRIGGER_THRESHOLD);
        status = (shouldBoomerang) ? State.Boomerang_Wait : State.Idle;
        cachedRigid.velocity *= TOSS_FORCE_DAMPENING;
    }

    private void ApplyDrag() {
        bool floating = (!cachedRigid.useGravity || Physics.gravity.sqrMagnitude <= Mathf.Epsilon);

        if(floating && cachedRigid.velocity.sqrMagnitude > IDLE_VELOCITY_THRESHOLD) {
            // Slow down object gradually when above the idle threshold, and there is no gravity.
            cachedRigid.AddForce(cachedRigid.velocity * DRAG_COEFFICIENT, ForceMode.VelocityChange);
        }
    }

    private void ApplyBoomerangForce() {
        Vector3 dirToTossedPos = tossedPos - cachedRigid.position;

        // Stop boomeranging when we pass the point/plane where we tossed from.
        // The boomerang path won't always be a straight line (bounces off walls),
        // so we can't use a distance threshold.
        float directionRelativeToToss = Vector3.Dot(-dirToTossedPos.normalized, tossedVelocity.normalized);

        if(directionRelativeToToss > 0f) {
            // Dot product so that we get the velocity relative to boomerang direction. It will be positive.
            float boomerangDot = Vector3.Dot(dirToTossedPos.normalized, cachedRigid.velocity);

            if(boomerangDot < BOOMERANG_VELOCITY_LIMIT) {
                cachedRigid.AddForce(dirToTossedPos * BOOMERANG_FORCE, ForceMode.Acceleration);
            }
        }
        else {
            status = State.Idle;
        }
    }
}