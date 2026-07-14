using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central, self-installing fix-up that runs on every scene so the app looks and
/// controls correctly on real phones without hand-editing 120+ scenes.
///
/// It applies, per scene:
///   • <see cref="UICanvasFitter"/> on every ScreenSpaceOverlay canvas — the UI
///     fills the screen edge-to-edge on tall phones (Problems 1 &amp; 2, UI part).
///   • <see cref="CameraFovFitter"/> on the main camera of celestial-body display
///     scenes — the planet is framed exactly as in the editor (Problem 2).
///   • <see cref="TouchDriveInput"/> + <see cref="TouchArrowControls"/> and hides the
///     OnScreenStick in rover/ship scenes — the vehicle is driven by the on-screen
///     arrow D-pad and/or by sliding a finger anywhere (Problem 4).
///
/// Installed once via <see cref="RuntimeInitializeOnLoadMethod"/>; hooks
/// SceneManager.sceneLoaded and also fixes the scene that is already active at boot.
/// All changes are runtime-only and idempotent (guarded against double-add).
/// </summary>
public static class MobileDisplayBootstrap
{
    private static bool installed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        if (installed) return;
        installed = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void FixInitialScene()
    {
        Apply(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    static void Apply(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return;

        bool isVehicleScene = false;
        bool isBodyScene = false;

        var roots = scene.GetRootGameObjects();

        // Detect scene kind from the components present.
        foreach (var root in roots)
        {
            if (root.GetComponentInChildren<PlanetAutoRotation>(true) != null)
                isBodyScene = true;
            if (root.GetComponentInChildren<ComplexRoverController>(true) != null
                || root.GetComponentInChildren<ShipFlightController>(true) != null
                || root.GetComponentInChildren<RoverController>(true) != null
                || root.GetComponentInChildren<AdvancedRoverController>(true) != null)
                isVehicleScene = true;
        }

        // 1) Canvas fit — every overlay canvas in every scene.
        foreach (var root in roots)
        {
            foreach (var scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                var canvas = scaler.GetComponent<Canvas>();
                if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                if (scaler.GetComponent<UICanvasFitter>() == null)
                    scaler.gameObject.AddComponent<UICanvasFitter>();
            }
        }

        // 2) Camera framing — celestial-body display scenes only (not gameplay/AR).
        if (isBodyScene && !isVehicleScene)
        {
            var cam = Camera.main;
            if (cam != null && !cam.orthographic && cam.GetComponent<CameraFovFitter>() == null)
                cam.gameObject.AddComponent<CameraFovFitter>();
        }

        // 3) Touch driving — rover/ship scenes. Both the finger-slide driver and the
        //    on-screen arrow D-pad feed the same TouchDriveInput contract, so the
        //    player can use whichever they prefer.
        if (isVehicleScene)
        {
            EnsureTouchDriveInput();
            EnsureTouchArrowControls();
            HideOnScreenSticks(roots);
        }
    }

    static void EnsureTouchDriveInput()
    {
        if (Object.FindFirstObjectByType<TouchDriveInput>() != null) return;
        var go = new GameObject("~TouchDriveInput");
        go.AddComponent<TouchDriveInput>();
    }

    static void EnsureTouchArrowControls()
    {
        if (Object.FindFirstObjectByType<TouchArrowControls>() != null) return;
        var go = new GameObject("~TouchArrowControls");
        go.AddComponent<TouchArrowControls>();
    }

    static void HideOnScreenSticks(GameObject[] roots)
    {
        var sticks = new List<OnScreenStick>();
        foreach (var root in roots)
            sticks.AddRange(root.GetComponentsInChildren<OnScreenStick>(true));

        foreach (var stick in sticks)
        {
            // Disable the whole joystick widget (handle + background) so nothing shows
            // and it stops feeding the Move action. Walk up to the outermost ancestor
            // whose name looks like a joystick container.
            Transform t = stick.transform;
            Transform toHide = t;
            Transform cursor = t;
            while (cursor != null && cursor.GetComponent<Canvas>() == null)
            {
                string n = cursor.name.ToLowerInvariant();
                if (n.Contains("joystick") || n.Contains("stick"))
                    toHide = cursor;
                cursor = cursor.parent;
            }
            toHide.gameObject.SetActive(false);
        }
    }
}
