using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controls Play/Pause for planet rotation and
// enables/disables the SoundButton depending on rotation state.
public class RotationToggle : MonoBehaviour
{
    // Reference to the rotation script
    public RotationController rotationController;

    // Reference to the planet AudioSource (same as SoundToggle)
    public AudioSource planetAudio;

    // UI references
    public Button rotationButton;
    public Button soundButton;              // 🔹 NEW (minimal addition)
    public TextMeshProUGUI buttonText;

    // Internal state
    private bool isPaused = false;

    private void Start()
    {
        // Initial state: rotation ON, sound button allowed
        buttonText.text = "Pause";

        if (soundButton != null)
            soundButton.interactable = true;

        rotationButton.onClick.AddListener(ToggleRotation);
    }

    private void ToggleRotation()
    {
        if (isPaused)
        {
            // ▶️ PLAY (resume rotation)
            rotationController.enabled = true;
            buttonText.text = "Pause";

            // Re-enable SoundButton
            if (soundButton != null)
                soundButton.interactable = true;

            // Resume sound ONLY if it was previously playing
            if (planetAudio != null && planetAudio.clip != null && !planetAudio.isPlaying)
            {
                planetAudio.UnPause();
            }
        }
        else
        {
            // ⏸️ PAUSE (stop rotation)
            rotationController.enabled = false;
            buttonText.text = "Play";

            // Disable SoundButton
            if (soundButton != null)
                soundButton.interactable = false;

            // Pause sound if it is currently playing
            if (planetAudio != null && planetAudio.isPlaying)
            {
                planetAudio.Pause();
            }
        }

        isPaused = !isPaused;
    }
}
