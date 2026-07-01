using UnityEngine;
using TMPro;
using System;

public class RoverHUD : MonoBehaviour
{
    public string planetName = "";
    public string environmentalInfo = "";

    [Header("Local Time Settings")]
    [Tooltip("Sidereal rotation period of this body in Earth hours. Drives the local day length and the running SOL counter. Use the magnitude; retrograde rotators are handled automatically.")]
    public float rotationPeriodHours = 24f;

    [Header("Telemetry Fields")]
    public TextMeshProUGUI earthDateTimeText;
    public TextMeshProUGUI localSolTimeText;
    public TextMeshProUGUI planetTelemetryText;
    public TextMeshProUGUI roverHealthText;

    // Fixed mission epoch (UTC). SOL 0 = this instant for every body; the counter
    // then advances at each body's own rotation rate.
    private static readonly DateTime MissionEpochUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    void Update()
    {
        DateTime now = DateTime.Now;
        if (earthDateTimeText != null) earthDateTimeText.text = $"<b>EARTH MISSION TIME</b>\nDATE: {now:yyyy-MM-dd}\nTIME: {now:HH:mm:ss}";

        if (localSolTimeText != null) {
            // Length of one local day in Earth seconds (abs so retrograde rotators work too).
            double dayLengthSeconds = System.Math.Abs(rotationPeriodHours) * 3600.0;
            if (dayLengthSeconds < 1.0) dayLengthSeconds = 24.0 * 3600.0; // safety fallback

            double elapsedEarthSeconds = (System.DateTime.UtcNow - MissionEpochUtc).TotalSeconds;
            double totalLocalDays = elapsedEarthSeconds / dayLengthSeconds;

            long sol = (long)System.Math.Floor(totalLocalDays);
            double fractionOfDay = totalLocalDays - sol; // 0..1 through the local day

            // Map the local day onto a 24h:60m:60s mission clock (like NASA's Coordinated Mars Time).
            double localSecondsOfDay = fractionOfDay * 24.0 * 3600.0;
            int h = (int)(localSecondsOfDay / 3600.0) % 24;
            int m = (int)(localSecondsOfDay / 60.0) % 60;
            int s = (int)(localSecondsOfDay % 60.0);
            localSolTimeText.text = $"<b>LOCAL MISSION CLOCK</b>\nSOL: {sol}\nTIME: {h:D2}:{m:D2}:{s:D2}";
        }

        if (planetTelemetryText != null) planetTelemetryText.text = $"<b>{planetName.ToUpper()} SENSOR DATA</b>\n{environmentalInfo}";
        
        if (roverHealthText != null) roverHealthText.text = $"<b>ROVER TELEMETRY</b>\nPOWER: 98.4%\nSIGNAL: NOMINAL\nTEMP: OPTIMAL";
    }
}