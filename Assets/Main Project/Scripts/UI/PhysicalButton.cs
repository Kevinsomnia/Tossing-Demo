using UnityEngine;

public class PhysicalButton : MonoBehaviour {
    public Transform buttonTrans;
    public Vector3 pushedOffset;
    public float transitionTime = 0.1f;
    public float physicalForceThreshold = 2f;
    public GameObject notifyTarget;
    public string notifyMessage = "MyFunction";

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

    private void OnCollisionStay(Collision collision) {
        if(collision.relativeVelocity.sqrMagnitude > physicalForceThreshold * physicalForceThreshold) {
            OnStartPushing();
        }
        else {
            // Force is too weak.
            OnStopPushing();
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
    }

    private void OnStopPushing() {
        if(!pressed)
            return;

        pressed = false;

        if(notifyTarget != null) {
            notifyTarget.SendMessage(notifyMessage);
        }
    }
}