using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class DemoUI : MonoBehaviour {
    public CameraController cameraLook;
    public ObjectSpawner spawner;
    public PostProcessLayer imageEffects;
    public Toggle settingsMenuToggle;
    public GameObject settingsMenuGo;
    public Toggle lowGraphicsToggle;
    public Toggle boomerangToggle;
    public Dropdown gravityDropdown;

    private void Awake() {
        settingsMenuToggle.isOn = false;
        settingsMenuGo.SetActive(false);
        lowGraphicsToggle.isOn = PlayerPrefs.GetInt("LowGraphics", 0) == 1;
        boomerangToggle.isOn = PlayerPrefs.GetInt("Boomerang", 1) == 1;
        gravityDropdown.value = PlayerPrefs.GetInt("Gravity", 0);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.M)) {
            settingsMenuToggle.isOn = !settingsMenuToggle.isOn;
        }
    }

    public void OnToggledSettingsMenu() {
        settingsMenuGo.SetActive(settingsMenuToggle.isOn);
        cameraLook.enabled = !settingsMenuToggle.isOn;
    }

    public void OnChangedLowGraphics() {
        if(lowGraphicsToggle.isOn) {
            QualitySettings.SetQualityLevel(0, false);
            imageEffects.enabled = false;
        }
        else {
            QualitySettings.SetQualityLevel(1, false);
            imageEffects.enabled = true;
        }

        PlayerPrefs.SetInt("LowGraphics", (lowGraphicsToggle.isOn) ? 1 : 0);
    }

    public void OnChangedBoomerang() {
        TossableObject.boomerangEnabled = boomerangToggle.isOn;
        PlayerPrefs.SetInt("Boomerang", (boomerangToggle.isOn) ? 1 : 0);
    }

    public void OnChangedGravity() {
        switch(gravityDropdown.value) {
            case 0:
                // No gravity.
                Physics.gravity = Vector3.zero;
                break;
            case 1:
                // Earth gravity.
                Physics.gravity = new Vector3(0f, -9.81f, 0f);
                break;
            case 2:
                // Moon gravity.
                Physics.gravity = new Vector3(0f, -1.62f, 0f);
                break;
            case 3:
                // Jupiter gravity.
                Physics.gravity = new Vector3(0f, -24.79f, 0f);
                break;
        }

        PlayerPrefs.SetInt("Gravity", gravityDropdown.value);
    }
    
    public void OnClickedSpawnObjects() {
        spawner.Spawn();
    }
}