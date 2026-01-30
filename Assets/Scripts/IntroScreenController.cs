using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// This class controls the behavior of the introductory screen.
// It displays the intro scene for a fixed amount of time,
// then automatically transitions to the main planets selection screen.
public class IntroScreenController : MonoBehaviour
{
    // Duration (in seconds) for which the intro screen is displayed
    // before loading the next scene.
    [SerializeField] private float displayTime = 5f;

    // Called once when the scene starts.
    // Begins the coroutine responsible for loading the next scene.
    private void Start()
    {
        StartCoroutine(LoadPlanetsScreen());
    }

    // Coroutine that waits for the specified display time
    // and then loads the planets selection scene.
    private IEnumerator LoadPlanetsScreen()
    {
        // Wait for the intro screen to remain visible
        yield return new WaitForSeconds(displayTime);

        // Load the planets selection screen.
        // The scene name must be added to Unity's Build Settings.
        SceneManager.LoadScene("PlanetsScreen");
    }
}
