using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void ExitApplication()
    {
        // Closes the app.
        Application.Quit();

        // It only works in the final build (not in the editor).
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
