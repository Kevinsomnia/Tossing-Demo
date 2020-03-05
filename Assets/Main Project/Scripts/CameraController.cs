using UnityEngine;

public class CameraController : MonoBehaviour {
    public float mouseSensitivity = 1f;
    public Vector2 xRotLimit = new Vector2(-70f, 70f);
    public Vector2 yRotLimit = new Vector2(-60f, 60f);

    private Transform cachedTrans;
    private float xRot;
    private float yRot;

    private void Awake() {
        cachedTrans = transform;
        xRot = cachedTrans.eulerAngles.y;
        yRot = cachedTrans.eulerAngles.x;
    }

    private void Update() {
        if(!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
            xRot += Input.GetAxis("Mouse X") * mouseSensitivity;
            yRot += Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        xRot = Mathf.Clamp(xRot, xRotLimit.x, xRotLimit.y);
        yRot = Mathf.Clamp(yRot, yRotLimit.x, yRotLimit.y);
        cachedTrans.rotation = Quaternion.Euler(-yRot, xRot, 0f);
    }
}