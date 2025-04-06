using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void ExitApplication()
    {
        // Închide aplicația
        Application.Quit();

        // Funcționează doar în build-ul final (nu în editor)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
