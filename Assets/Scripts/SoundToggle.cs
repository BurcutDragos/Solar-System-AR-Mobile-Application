using UnityEngine;
using UnityEngine.UI;
using TMPro;  // <-- Importă biblioteca pentru TextMeshPro


public class SoundToggle : MonoBehaviour
{
    public AudioSource planetAudio; // Referință către AudioSource-ul planetei
    public Button soundButton; // Referință către buton
    public TextMeshProUGUI buttonText; // Referință către textul butonului

    private bool isMuted = true; // Variabilă pentru a verifica dacă sunetul e oprit

    void Start()
    {
        // Setează textul inițial
        buttonText.text = "Unmute";

        // Adaugă listener pe buton
        soundButton.onClick.AddListener(ToggleSound);
    }

    void ToggleSound()
    {
        if (isMuted)
        {
            planetAudio.Play(); // Pornește sunetul
            buttonText.text = "Mute"; // Schimbă textul
        }
        else
        {
            planetAudio.Stop(); // Oprește sunetul
            buttonText.text = "Unmute"; // Schimbă textul
        }

        isMuted = !isMuted; // Inversează starea
    }
}
