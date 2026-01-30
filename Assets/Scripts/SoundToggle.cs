using TMPro;  // <-- Imports the library for TextMeshPro.
using UnityEngine;
using UnityEngine.UI;


public class SoundToggle : MonoBehaviour
{
    public AudioSource planetAudio; // Reference to the planet's AudioSource.
    public Button soundButton; // Reference to the soundButton.
    public TextMeshProUGUI buttonText; // Reference to the button text.

    private bool isMuted = true; // Variable to check if the sound is muted.

    void Start()
    {
        if (buttonText == null)
        {
            Debug.LogError("SoundToggle: buttonText NU este asignat!", this);
            return;
        }

        if (soundButton == null)
        {
            Debug.LogError("SoundToggle: soundButton NU este asignat!", this);
            return;
        }

        if (planetAudio == null)
        {
            Debug.LogWarning("SoundToggle: planetAudio NU este asignat!", this);
        }

        /*
         * if (soundButton != null)
                buttonText = soundButton.GetComponentInChildren<TextMeshProUGUI>();
         */

        // Sets the initial text.
        buttonText.text = "Unmute";

        // Add listener to the button.
        soundButton.onClick.AddListener(ToggleSound);
    }

    void ToggleSound()
    {
        if (isMuted)
        {
            planetAudio.Play(); // Turns on the sound.
            buttonText.text = "Mute"; // Changes the text.
        }
        else
        {
            planetAudio.Stop(); // Turns off the sound.
            buttonText.text = "Unmute"; // Changes the text.
        }

        isMuted = !isMuted; // Reverses the state.
    }
}
