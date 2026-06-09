using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpaceShipHUD : MonoBehaviour
{
    [Header("UI References")]
    public GameObject topBar; // Added reference to the top bar
    public TextMeshProUGUI shipStatsText;
    public TextMeshProUGUI environmentalText;
    public TextMeshProUGUI planetInfoText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Image gameOverImage;
    public Button restartButton;
    public Button backButton;

    [Header("Ship Reference")]
    public SpaceShipSurfaceFlight ship;

    [Header("Labels")]
    public string statsLabel = "ShipStats";
    public string envLabel = "EnvironmentalInfo";
    public string planetLabel = "PlanetInfo";

    private void Start()
    {
        // Detect scene for planet label
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Mars")) planetLabel = "MarsInfo";
        else if (sceneName.Contains("Mercury")) planetLabel = "MercuryInfo";
        else if (sceneName.Contains("Pluto")) planetLabel = "PlutoInfo";

        if (ship != null)
        {
            ship.OnCrash += ShowGameOver;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (topBar != null) topBar.SetActive(true); // Ensure top bar is on at start

        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    private void Update()
    {
        UpdateDateTime();
        UpdateShipStats();
        UpdatePlanetInfo();
    }

    private void UpdateDateTime()
    {
        if (environmentalText == null) return;
        DateTime now = DateTime.Now;
        environmentalText.text = string.Format(
            "<color=yellow>ENVIRONMENT</color>\nDATE: {0:dd/MM/yyyy}\nTIME: {0:HH:mm:ss}", 
            now);
    }

    private void UpdateShipStats()
    {
        if (shipStatsText == null || ship == null) return;
        
        float speed = ship.CurrentSpeed;
        float altitude = ship.CurrentAltitude;
        float inclination = ship.CurrentInclination;

        shipStatsText.text = string.Format(
            "<color=yellow>SPACECRAFT</color>\nSPEED: {0:F1} u/s\nALTITUDE: {1:F1} u\nINCLINATION: {2:F1}°",
            speed, altitude, inclination);
    }

    private void UpdatePlanetInfo()
    {
        if (planetInfoText == null) return;

        string sceneName = SceneManager.GetActiveScene().name;
        string gravity, temp, pressure, composition, planetName;

        if (sceneName.Contains("Mars"))
        {
            planetName = "MARS";
            gravity = "3.71 m/s² (0.376g)";
            temp = "Avg -60°C";
            pressure = "610 Pa (6 mbar)";
            composition = "95% CO2, 2.7% N2, 1.6% Ar";
        }
        else if (sceneName.Contains("Mercury"))
        {
            planetName = "MERCURY";
            gravity = "3.70 m/s² (0.378g)";
            temp = "Avg 167°C";
            pressure = "1 nPa (Trace)";
            composition = "Oxygen, Sodium, Hydrogen";
        }
        else
        {
            planetName = "PLUTO";
            gravity = "0.62 m/s² (0.063g)";
            temp = "-230°C";
            pressure = "1.0 Pa (10 μbar)";
            composition = "98% N2, 1.5% CH4, 0.5% CO";
        }

        planetInfoText.text = string.Format(
            "<color=yellow>{0} DATA</color>\nGRAVITY: {1}\nTEMP: {2}\nPRESSURE: {3}\nATMOSPHERE: {4}",
            planetName, gravity, temp, pressure, composition);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(false);
            }
            if (gameOverImage != null)
            {
                gameOverImage.gameObject.SetActive(true);
                gameOverImage.color = Color.white;
            }
        }
        
        if (topBar != null) topBar.SetActive(false); // Disable top bar when crashed
    }

    private void OnRestartClicked()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (topBar != null) topBar.SetActive(true); // Re-enable top bar on restart
        if (ship != null) ship.ResetFlight();
    }

    private void OnBackClicked()
    {
        SceneManager.LoadScene("PlanetsScreen");
    }
}