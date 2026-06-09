using UnityEngine;
using TMPro;
using System;

public class RoverHUD : MonoBehaviour
{
    public string planetName = "";
    public string environmentalInfo = "";

    [Header("Telemetry Fields")]
    public TextMeshProUGUI earthDateTimeText;
    public TextMeshProUGUI localSolTimeText;
    public TextMeshProUGUI planetTelemetryText;
    public TextMeshProUGUI roverHealthText;

    private float localSolOffset;

    void Start()
    {
        localSolOffset = UnityEngine.Random.Range(0f, 24f);
    }

    void Update()
    {
        DateTime now = DateTime.Now;
        if (earthDateTimeText != null) earthDateTimeText.text = $"<b>EARTH MISSION TIME</b>\nDATE: {now:yyyy-MM-dd}\nTIME: {now:HH:mm:ss}";
        
        if (localSolTimeText != null) {
            float t = (float)now.TimeOfDay.TotalSeconds + (localSolOffset * 3600);
            int sol = 142; 
            int h = (int)(t / 3600) % 24;
            int m = (int)(t / 60) % 60;
            int s = (int)(t % 60);
            localSolTimeText.text = $"<b>LOCAL MISSION CLOCK</b>\nSOL: {sol}\nTIME: {h:D2}:{m:D2}:{s:D2}";
        }

        if (planetTelemetryText != null) planetTelemetryText.text = $"<b>{planetName.ToUpper()} SENSOR DATA</b>\n{environmentalInfo}";
        
        if (roverHealthText != null) roverHealthText.text = $"<b>ROVER TELEMETRY</b>\nPOWER: 98.4%\nSIGNAL: NOMINAL\nTEMP: OPTIMAL";
    }
}