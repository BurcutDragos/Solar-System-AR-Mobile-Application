using UnityEngine;
using TMPro;
using System;

/// <summary>
/// HUD for the gas-giant ship screens. Mirrors RoverHUD's display pattern
/// (real Earth time + a per-body "local mission clock" scaled to the body's
/// rotation period, plus atmosphere sensor data) but with ship-appropriate
/// telemetry pulled live from a <see cref="ShipFlightController"/>.
/// </summary>
public class ShipHUD : MonoBehaviour
{
    public string planetName = "";
    public string environmentalInfo = "";

    [Header("Local Time Settings")]
    [Tooltip("Sidereal rotation period of this body in Earth hours. Drives the local day length and running SOL counter.")]
    public float rotationPeriodHours = 24f;

    [Header("Telemetry Fields")]
    public TextMeshProUGUI earthDateTimeText;
    public TextMeshProUGUI localSolTimeText;
    public TextMeshProUGUI planetTelemetryText;
    public TextMeshProUGUI shipTelemetryText;

    [Header("Live Data Source")]
    public ShipFlightController ship;

    private static readonly DateTime MissionEpochUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    void Update()
    {
        DateTime now = DateTime.Now;
        if (earthDateTimeText != null)
            earthDateTimeText.text = $"<b>EARTH MISSION TIME</b>\nDATE: {now:yyyy-MM-dd}\nTIME: {now:HH:mm:ss}";

        if (localSolTimeText != null)
        {
            double dayLengthSeconds = System.Math.Abs(rotationPeriodHours) * 3600.0;
            if (dayLengthSeconds < 1.0) dayLengthSeconds = 24.0 * 3600.0;

            double elapsedEarthSeconds = (System.DateTime.UtcNow - MissionEpochUtc).TotalSeconds;
            double totalLocalDays = elapsedEarthSeconds / dayLengthSeconds;

            long sol = (long)System.Math.Floor(totalLocalDays);
            double fractionOfDay = totalLocalDays - sol;
            double localSecondsOfDay = fractionOfDay * 24.0 * 3600.0;
            int h = (int)(localSecondsOfDay / 3600.0) % 24;
            int m = (int)(localSecondsOfDay / 60.0) % 60;
            int s = (int)(localSecondsOfDay % 60.0);
            localSolTimeText.text = $"<b>LOCAL MISSION CLOCK</b>\nSOL: {sol}\nTIME: {h:D2}:{m:D2}:{s:D2}";
        }

        if (planetTelemetryText != null)
            planetTelemetryText.text = $"<b>{planetName.ToUpper()} ATMOSPHERE</b>\n{environmentalInfo}";

        if (shipTelemetryText != null)
        {
            float spd = ship != null ? ship.CurrentSpeed : 0f;
            float altKm = (ship != null ? ship.CurrentAltitude : 0f) * 0.1f; // stylised depth readout
            shipTelemetryText.text = $"<b>SHIP TELEMETRY</b>\nVELOCITY: {spd:0} m/s\nDEPTH: {altKm:0} km\nHULL: NOMINAL";
        }
    }
}
