using UnityEngine;

public class RobotEyes : MonoBehaviour {
    public AnimationCurve blinkAnim;
    public Transform leftEye;
    public Transform rightEye;
    public float blinkSpeed = 10f;
    public Vector2 blinkInterval = new Vector2(0.25f, 3f);

    private bool blinking;
    private float animTimer;
    private float nextBlinkTime;

    private void Awake() {
        blinking = false;
        animTimer = 0f;
    }

    private void Update() {
        if(blinking) {
            animTimer += Time.deltaTime * blinkSpeed;

            if(animTimer >= 1f) {
                blinking = false;
                animTimer = 0f;
            }
        }
        else {
            if(Time.time >= nextBlinkTime) {
                Blink();
            }
        }

        // Update eye scale to simulate blinking.
        Vector3 scale = leftEye.localScale;
        scale.z = blinkAnim.Evaluate(animTimer);
        leftEye.localScale = scale;
        rightEye.localScale = scale;
    }

    public void Blink() {
        if(blinking)
            return;

        blinking = true;
        animTimer = 0f;
        nextBlinkTime = Time.time + Random.Range(blinkInterval.x, blinkInterval.y);
    }
}