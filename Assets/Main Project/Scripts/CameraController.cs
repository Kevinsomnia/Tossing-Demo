﻿using UnityEngine;

public class CameraController : MonoBehaviour {
    public float mouseSensitivity = 1f;

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
            yRot -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        }

        xRot = Mathf.Clamp(xRot, -70f, 70f);
        yRot = Mathf.Clamp(yRot, -60f, 60f);
        cachedTrans.rotation = Quaternion.Euler(yRot, xRot, 0f);
    }
}
