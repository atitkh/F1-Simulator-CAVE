using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TelemetryData : MonoBehaviour
{
    public SimController simController;
    public GameObject telemetryDataPanel;

    private void Start()
    {
        simController.OnTelemetryUpdated.AddListener(OnTelemetryUpdated);
    }

    private void Update()
    {
        // if (simController.isSimulating)
        // {
        //     simController.selectDriver(1);
        // }
    }

    private void OnTelemetryUpdated()
    {
        //get the selected driver and track data
        if (simController.selectedDriver == null || simController.selectedDriverTrackData == null)
        {
            return;
        }

        Driver driver = simController.selectedDriver;
        TrackData trackData = simController.selectedDriverTrackData;
        Debug.Log("Telemetry updated for driver: " + driver.full_name);
        // Update telemetry data
        telemetryDataPanel.transform.Find("Name").GetComponent<TMP_Text>().text = driver.full_name.Split(' ')[1];
        telemetryDataPanel.transform.Find("RPM").GetComponent<TMP_Text>().text = "RPM: " + trackData.carData.rpm;
        telemetryDataPanel.transform.Find("Speed").GetComponent<TMP_Text>().text = "Speed: " + trackData.carData.speed;
        telemetryDataPanel.transform.Find("Gear").GetComponent<TMP_Text>().text = "Gear: " + trackData.carData.n_gear;
        telemetryDataPanel.transform.Find("Throttle").GetComponent<TMP_Text>().text = "Throttle: " + trackData.carData.throttle;
        telemetryDataPanel.transform.Find("Brake").GetComponent<TMP_Text>().text = "Brake: " + trackData.carData.brake;

        var drs = trackData.carData.drs switch
        {
            0 => "DRS off",
            1 => "DRS off",
            2 => "?",
            3 => "?",
            8 => "Detected",
            9 => "?",
            10 => "DRS on",
            12 => "DRS on",
            14 => "DRS on",
            _ => "Unknown"
        };
        telemetryDataPanel.transform.Find("DRS").GetComponent<TMP_Text>().text = "DRS: " + drs;
    }
}