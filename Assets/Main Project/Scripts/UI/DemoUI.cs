using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DemoUI : MonoBehaviour {
    public CameraController cameraLook;
    public ObjectSpawner spawner;
    public PostProcessLayer imageEffects;
    public PhysicalButton lowGraphicsButton;
    public PhysicalButton boomerangButton;
    public PhysicalButton[] gravityButtons;

    private bool lowGraphics;
    private bool boomerang;
    private string gravityOption;

    private void Awake() {
        lowGraphics = (PlayerPrefs.GetInt("LowGraphics", 0) == 1);
        boomerang = (PlayerPrefs.GetInt("Boomerang", 1) == 1);
        gravityOption = PlayerPrefs.GetString("Gravity", "None");
        ApplySettings();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void OnClickedLowGraphics() {
        lowGraphics = !lowGraphics;
        PlayerPrefs.SetInt("LowGraphics", (lowGraphics) ? 1 : 0);
        ApplySettings();
    }

    public void OnClickedBoomerang() {
        boomerang = !boomerang;
        PlayerPrefs.SetInt("Boomerang", (boomerang) ? 1 : 0);
        ApplySettings();
    }

    public void OnClickedGravity(string option) {
        gravityOption = option;
        PlayerPrefs.SetString("Gravity", option);
        ApplySettings();
    }

    public void OnClickedSpawnObjects() {
        spawner.Spawn();
    }

    private void ApplySettings() {
        // Graphic settings.
        if(lowGraphics) {
            QualitySettings.SetQualityLevel(0, false);
            imageEffects.enabled = false;
        }
        else {
            QualitySettings.SetQualityLevel(1, false);
            imageEffects.enabled = true;
        }

        lowGraphicsButton.SetRingState(lowGraphics);

        // Boomerang
        TossableObject.boomerangEnabled = boomerang;
        boomerangButton.SetRingState(boomerang);

        // Gravity.
        int buttonIndex = 0;

        switch(gravityOption) {
            case "None":
                Physics.gravity = Vector3.zero;
                buttonIndex = 0;
                break;
            case "Earth":
                Physics.gravity = new Vector3(0f, -9.81f, 0f);
                buttonIndex = 1;
                break;
            case "Moon":
                Physics.gravity = new Vector3(0f, -1.62f, 0f);
                buttonIndex = 2;
                break;
            case "Jupiter":
                Physics.gravity = new Vector3(0f, -24.79f, 0f);
                buttonIndex = 3;
                break;
        }

        for(int i = 0; i < gravityButtons.Length; i++) {
            gravityButtons[i].SetRingState(i == buttonIndex);
        }
    }
}