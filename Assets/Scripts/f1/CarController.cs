using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private SimController simController;
    public string DriverAcronym;
    private Driver driver;
    private TrackData trackData;

    // Start is called before the first frame update
    void Start()
    {
        simController.OnTelemetryUpdated.AddListener(OnTelemetryUpdated);
        GetComponentInChildren<TMP_Text>().text = DriverAcronym;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTelemetryUpdated()
    {
        if (simController.selectedDriver == null || simController.selectedDriverTrackData == null)
        {
            return;
        }

        Driver driver = simController.selectedDriver;
        TrackData trackData = simController.selectedDriverTrackData;
    }
}
