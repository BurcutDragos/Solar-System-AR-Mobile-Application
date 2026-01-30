using UnityEngine;
using UnityEngine.SceneManagement;

// This class is responsible for handling scene transitions within the application.
// It provides a simple interface for loading different Unity scenes by name.
// Typically used for menu buttons or navigation between different celestial bodies.
public class SceneController : MonoBehaviour
{
    // Loads a new scene based on the provided scene name.
    // This method can be linked directly to UI Buttons via the Unity Inspector,
    // passing the target scene's name as a parameter.
    public void SwitchScenes(string sceneName)
    {
        // Loads the scene with the specified name.
        // The scene must be added to the Build Settings in Unity.
        SceneManager.LoadScene(sceneName);
    }
}
