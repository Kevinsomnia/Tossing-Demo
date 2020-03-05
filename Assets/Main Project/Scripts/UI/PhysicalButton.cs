using UnityEngine;

public class PhysicalButton : MonoBehaviour {
    private const float PUSH_DOT_THRESHOLD = 0.9f;

    public Transform buttonTrans;
    public Transform pushDirection;
    public Renderer ringRenderer;
    public Vector3 pushedOffset;
    public float transitionTime = 0.1f;
    public float physicalForceThreshold = 2f;
    public GameObject notifyTarget;
    public string notifyMessage = "MyFunction";
    public string notifyValue = "SomeValue";

    private Vector3 defaultPos;
    private float animTime;
    private bool pressed;

    private void Awake() {
        pressed = false;
        defaultPos = buttonTrans.localPosition;
    }

    private void Update() {
        animTime = Mathf.MoveTowards(animTime, (pressed) ? 1f : 0f, Time.deltaTime / transitionTime);
        buttonTrans.localPosition = defaultPos + (pushedOffset * animTime);
    }

    public void SetRingState(bool on) {
        if(ringRenderer != null) {
            ringRenderer.enabled = on;
        }
    }

    private void OnCollisionStay(Collision collision) {
        // Push once enough force is applied at the correct angle.
        if(collision.relativeVelocity.sqrMagnitude > physicalForceThreshold * physicalForceThreshold) {
            float velocityDot = Vector3.Dot(collision.relativeVelocity.normalized, pushDirection.forward);

            if(velocityDot >= PUSH_DOT_THRESHOLD) {
                OnStartPushing();
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        OnStopPushing();
    }

    private void OnMouseDown() {
        OnStartPushing();
    }

    private void OnMouseUp() {
        OnStopPushing();
    }

    private void OnMouseExit() {
        OnStopPushing();
    }

    private void OnStartPushing() {
        if(pressed)
            return;

        pressed = true;

        if(notifyTarget != null) {
            notifyTarget.SendMessage(notifyMessage, notifyValue);
        }
    }

    private void OnStopPushing() {
        if(!pressed)
            return;

        pressed = false;
    }
}