using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RotationToggle : MonoBehaviour
{
    public PlanetSceneController sceneController;
    public AudioSource planetAudio;

    public Button rotationButton;
    public Button soundButton;
    public Button resetButton;
    public TextMeshProUGUI buttonText;

    private void Start()
    {
        buttonText.text = "Pause";
        rotationButton.onClick.AddListener(ToggleRotation);
    }

    private void ToggleRotation()
    {
        sceneController.TogglePause();

        bool paused = sceneController.IsPaused;

        buttonText.text = paused ? "Play" : "Pause";

        soundButton.interactable = !paused;
        resetButton.interactable = !paused;

        if (planetAudio != null)
        {
            if (paused)
                planetAudio.Pause();
            else
                planetAudio.UnPause();
        }
    }
}