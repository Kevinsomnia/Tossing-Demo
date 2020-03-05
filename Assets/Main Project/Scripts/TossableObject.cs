using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TossableObject : MonoBehaviour {
    // Global constants for all tossables.
    private const float DRAG_COEFFICIENT = -0.06f;
    private const float IDLE_VELOCITY_THRESHOLD = 0.02f * 0.02f;
    private const float TOSS_FORCE_DAMPENING = 0.75f;
    private const float BOOMERANG_ARC = 335f;                           // Length of boomerang path in degrees.
    private const float BOOMERANG_DURATION = 4f;                        // Duration of the boomerang cycle.
    private const float BOOMERANG_TRIGGER_THRESHOLD = 0.4f;             // Minimum toss velocity in order to start boomeranging.
    private const float BOOMERANG_FORCE = 0.3f;                         // Velocity multiplier during boomerang.
    private const float BOOMERANG_SPIN = 2.5f;                          // Torque multiplier during boomerang.
    private const float BOOMERANG_VELOCITY_LIMIT = 3f;                  // Prevent boomerang from coming back too fast.

    public static bool boomerangEnabled = true;

    public enum State { Idle, Boomeranging, Grabbed };

    public Transform cachedTrans { get; private set; }
    public Rigidbody cachedRigid { get; private set; }

    private State status;
    private Vector3 tossedPos;
    private Vector3 tossedVelocity;
    private float tossedVelocityMagnitude;
    private float prevBoomerangSpeed;                                   // Used for boomeranging to detect abrupt deceleration.
    private float lastTossedTime;
    private float boomerangAngle;
    private bool boomerangingCCW;

    private void Awake() {
        cachedTrans = transform;
        cachedRigid = GetComponent<Rigidbody>();
        status = State.Idle;
    }

    private void FixedUpdate() {
        if(status != State.Grabbed && status != State.Boomeranging) {
            ApplyDrag();
        }

        // Boomerang force is too weak to fight against gravity (if something other than no gravity is selected).
        cachedRigid.useGravity = (status != State.Boomeranging);

        if(status == State.Boomeranging) {
            // Apply boomerang force.
            ApplyBoomerangForce();
        }
    }

    public void OnGrabbed() {
        status = State.Grabbed;
    }

    public void OnTossed() {
        cachedRigid.velocity *= TOSS_FORCE_DAMPENING;

        tossedPos = cachedRigid.position;
        tossedVelocity = cachedRigid.velocity;
        tossedVelocityMagnitude = tossedVelocity.magnitude;
        lastTossedTime = Time.time;

        // Check that boomeranging is enabled and tossed hard enough.
        bool shouldBoomerang = (boomerangEnabled && tossedVelocityMagnitude >= BOOMERANG_TRIGGER_THRESHOLD);

        if(shouldBoomerang) {
            status = State.Boomeranging;
            boomerangAngle = 0f;
            boomerangingCCW = tossedVelocity.x > 0f; // Throw right = rotates CCW.
            prevBoomerangSpeed = tossedVelocityMagnitude;
        }
        else {
            status = State.Idle;
        }
    }

    private void ApplyDrag() {
        bool floating = (!cachedRigid.useGravity || Physics.gravity.sqrMagnitude <= Mathf.Epsilon);

        if(floating && cachedRigid.velocity.sqrMagnitude > IDLE_VELOCITY_THRESHOLD) {
            // Slow down object gradually when above the idle threshold, and there is no gravity.
            cachedRigid.AddForce(cachedRigid.velocity * DRAG_COEFFICIENT, ForceMode.VelocityChange);
        }
    }

    private void ApplyBoomerangForce() {
        if(boomerangingCCW)
            // Threw right. Counter clockwise path.
            boomerangAngle -= Time.deltaTime * (BOOMERANG_ARC / BOOMERANG_DURATION);
        else
            // Threw left. Clockwise path.
            boomerangAngle += Time.deltaTime * (BOOMERANG_ARC / BOOMERANG_DURATION);

        // Initial velocity is direction of toss, then rotate the velocity vector around using quaternion multiplication.
        // This will create a circular path.
        Quaternion boomerangRot = Quaternion.LookRotation(tossedVelocity) * Quaternion.Euler(0f, boomerangAngle, 0f);
        Vector3 boomerangDir = boomerangRot * Vector3.forward;

        if(Mathf.Abs(boomerangAngle) > BOOMERANG_ARC * 0.75f) {
            // Pull closer to the tossed position when nearing the end of the boomerang path.
            Vector3 dirToTossedPos = tossedPos - cachedRigid.position;
            boomerangDir += dirToTossedPos.normalized * 0.05f;
        }
        
        // If we hit an object while boomeranging and slow down significantly, avoid sticking to the surface.
        if(prevBoomerangSpeed > Mathf.Epsilon) {
            // Factor of deceleration since last frame (0 = didnt slow down, 1 = stopped completely).
            float decelFactor = (prevBoomerangSpeed - cachedRigid.velocity.magnitude) / prevBoomerangSpeed;

            if(decelFactor > 0.66f) {
                // Slowed down by more than 66% since last frame. Stop boomeranging.
                status = State.Idle;
                return;
            }
        }
        
        cachedRigid.velocity = boomerangDir * BOOMERANG_FORCE * tossedVelocityMagnitude;
        prevBoomerangSpeed = cachedRigid.velocity.magnitude;

        cachedRigid.AddTorque(cachedTrans.forward * 2.5f * tossedVelocityMagnitude, ForceMode.Acceleration);

        if(Mathf.Abs(boomerangAngle) > BOOMERANG_ARC) {
            // End of boomerang path.
            status = State.Idle;
        }
    }
}