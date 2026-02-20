using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RotationToggle : MonoBehaviour
{
    public RotationController rotationController;
    public AudioSource planetAudio;

    public Button rotationButton;
    public Button soundButton;
    public Button resetButton;        // 🔹 NEW
    public TextMeshProUGUI buttonText;

    private bool isPaused = false;

    private void Start()
    {
        buttonText.text = "Pause";

        if (soundButton != null)
            soundButton.interactable = true;

        if (resetButton != null)      // 🔹 NEW
            resetButton.interactable = true;

        rotationButton.onClick.AddListener(ToggleRotation);
    }

    private void ToggleRotation()
    {
        if (isPaused)
        {
            // ▶️ PLAY
            rotationController.enabled = true;
            buttonText.text = "Pause";

            if (soundButton != null)
                soundButton.interactable = true;

            if (resetButton != null)          // 🔹 NEW
                resetButton.interactable = true;

            if (planetAudio != null && planetAudio.clip != null && !planetAudio.isPlaying)
            {
                planetAudio.UnPause();
            }
        }
        else
        {
            // ⏸️ PAUSE
            rotationController.enabled = false;
            buttonText.text = "Play";

            if (soundButton != null)
                soundButton.interactable = false;

            if (resetButton != null)          // 🔹 NEW
                resetButton.interactable = false;

            if (planetAudio != null && planetAudio.isPlaying)
            {
                planetAudio.Pause();
            }
        }

        isPaused = !isPaused;
    }
}
