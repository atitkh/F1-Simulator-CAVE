using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimController : MonoBehaviour
{
    // Public Fields
    public F1Data F1Data;
    public string sessionKey = "9507";
    public GameObject carPrefab;
    public GameObject carParent;
    public Driver selectedDriver;
    public TrackData selectedDriverTrackData;

    // Private Fields
    private List<Driver> drivers = new();
    private DateTime currentTime;

    // Hidden in Inspector
    [HideInInspector]
    public Dictionary<int, GameObject> carModels = new Dictionary<int, GameObject>();
    [HideInInspector]
    public string startTime = "2024-05-05T20:00:00+00:00"; // start time of the session
    [HideInInspector]
    public string endTime = "2024-05-05T22:00:00+00:00"; // end time of the session
    [HideInInspector]
    public Dictionary<int, Interval> leaderboard = new Dictionary<int, Interval>();
    [HideInInspector]
    public DateTime simulatedTime;
    [HideInInspector]
    public int timeMultiplier = 1; // Time speed-up factor for the simulation
    [HideInInspector]
    public bool isSimulating = false;

    // Unity Events
    #region Unity Events 
    public UnityEvent OnLeaderboardUpdated = new UnityEvent();
    public UnityEvent OnTelemetryUpdated = new UnityEvent();
    #endregion

    private void Start()
    {
        F1Data.OnDataFetched.AddListener(OnDataFetched);
        F1Data.GetAllDrivers(sessionKey, startTime, endTime);
    }

    private void Update()
    {
        if (isSimulating)
        {
            SimulateTime();
            UpdateCarPositions();
        }
    }

    private void OnDataFetched()
    {
        drivers = F1Data.AllDrivers();
        foreach (Driver driver in drivers)
        {
            LoadCar(driver.driver_number, driver.name_acronym);
        }
        selectDriver(1);
        StartSimulation();
    }

    private void LoadCar(int driver_number, string driver_acronym = "DRIVER")
    {
        // new parent object for the car
        GameObject driverParent = new GameObject($"Car_{driver_number}");
        driverParent.transform.parent = carParent.transform;
        driverParent.transform.position = Vector3.zero;
        driverParent.transform.localScale = Vector3.one;
        GameObject car = Instantiate(carPrefab, Vector3.zero, Quaternion.identity, driverParent.transform);
        carModels.Add(driver_number, driverParent);

        // Set the driver acronym
        car.GetComponent<CarController>().DriverAcronym = driver_acronym;
    }

    private void loadLeaderboard()
    {
        foreach (KeyValuePair<int, Interval> entry in leaderboard)
        {
            Debug.Log($"Driver {entry.Key} is {entry.Value.gap_to_leader} seconds behind the leader.");
        }
    }

    public void selectDriver(int driver_number)
    {
        selectedDriver = F1Data.GetDriver(driver_number);
        Debug.Log($"Selected driver: {selectedDriver.full_name}");
    }

    public void StartSimulation()
    {
        Debug.Log("Starting simulation");
        simulatedTime = DateTime.Parse(startTime);
        currentTime = DateTime.Now;
        isSimulating = true;
    }

    public void StopSimulation()
    {
        Debug.Log("Stopping simulation");
        isSimulating = false;
    }

    public void PausePlaySimulation()
    {
        isSimulating = !isSimulating;
    }

    public void ResetSimulation()
    {
        Debug.Log("Resetting simulation");
        simulatedTime = DateTime.Parse(startTime);
        currentTime = DateTime.Now;
        timeMultiplier = 1;
        isSimulating = false;
    }

    private void SimulateTime()
    {
        // Update simulated time based on real-time progression and multiplier
        simulatedTime = simulatedTime.AddSeconds(Time.deltaTime * timeMultiplier);

        // Stop simulation if it reaches the end time
        if (simulatedTime >= DateTime.Parse(endTime))
        {
            isSimulating = false;
            Debug.Log("Simulation ended.");
        }
    }

    private void UpdateCarPositions()
    {
        foreach (Driver driver in drivers)
        {
            if (carModels.ContainsKey(driver.driver_number))
            {
                List<TrackData> trackData = F1Data.GetAllTrackData(driver.driver_number);
                if (trackData != null && trackData.Count > 1)
                {
                    UpdateCarPosition(driver.driver_number, trackData);
                }
            }
        }
    }

    private void UpdateCarPosition(int driverNumber, List<TrackData> trackData)
    {
        if (trackData == null || trackData.Count == 0)
        {
            Debug.LogWarning($"Track data is null or empty for Driver {driverNumber}");
            return;
        }

        int prevIndex = -1, nextIndex = -1;
        int left = 0, right = trackData.Count - 1;

        // Perform binary search to find the correct interval
        while (left <= right)
        {
            int mid = (left + right) / 2;
            DateTime midTime = DateTime.Parse(trackData[mid].date);

            if (midTime <= simulatedTime)
            {
                if (mid + 1 < trackData.Count && DateTime.Parse(trackData[mid + 1].date) > simulatedTime)
                {
                    prevIndex = mid;
                    nextIndex = mid + 1;
                    break;
                }
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        if (prevIndex >= 0 && nextIndex >= 0)
        {
            UpdateLeaderboard(driverNumber, trackData[prevIndex]);
            UpdateTelemetryData(driverNumber, trackData[prevIndex]);
            InterpolatePosition(driverNumber, trackData[prevIndex], trackData[nextIndex]);
        }
    }

    private void InterpolatePosition(int driverNumber, TrackData prevData, TrackData nextData)
    {
        float timeBetween = (float)(DateTime.Parse(nextData.date) - DateTime.Parse(prevData.date)).TotalSeconds;
        float timeSincePrev = (float)(simulatedTime - DateTime.Parse(prevData.date)).TotalSeconds;
        float t = Mathf.Clamp01(timeSincePrev / timeBetween);

        // Interpolate position
        Vector3 prevPosition = new Vector3(prevData.x, 0, prevData.y);
        Vector3 nextPosition = new Vector3(nextData.x, 0, nextData.y);
        Vector3 interpolatedPosition = Vector3.Lerp(prevPosition, nextPosition, t);
        carModels[driverNumber].transform.localPosition = interpolatedPosition;

        // Interpolate rotation
        float angle = Mathf.Atan2(nextPosition.x - prevPosition.x, nextPosition.z - prevPosition.z) * Mathf.Rad2Deg;
        carModels[driverNumber].transform.localRotation = Quaternion.Slerp(carModels[driverNumber].transform.localRotation, Quaternion.Euler(0, angle, 0), t);

        // Debug.Log($"Updated position for Driver {driverNumber} to {interpolatedPosition}");
    }

    private void UpdateTelemetryData(int driverNumber, TrackData trackData)
    {
        if (selectedDriver != null && selectedDriver.driver_number == driverNumber)
        {
            selectedDriverTrackData = trackData;
            OnTelemetryUpdated.Invoke();
        }
    }

    private void UpdateLeaderboard(int driverNumber, TrackData trackData)
    {
        if (leaderboard.ContainsKey(driverNumber))
        {
            if (leaderboard[driverNumber] != trackData.intervalData)
            {
                leaderboard[driverNumber] = trackData.intervalData;
                leaderboard = leaderboard
                                .OrderBy(x => x.Value.gap_to_leader)
                                .Select((x, index) =>
                                {
                                    x.Value.position = index + 1; // add position to the interval data
                                    return x;
                                })
                                .ToDictionary(x => x.Key, x => x.Value);
                OnLeaderboardUpdated.Invoke();
            }
        }
        else
        {
            leaderboard.Add(driverNumber, trackData.intervalData);
            leaderboard = leaderboard.OrderBy(x => x.Value.gap_to_leader)
                            .Select((x, index) =>
                            {
                                x.Value.position = index + 1; // add position to the interval data
                                return x;
                            })
                            .ToDictionary(x => x.Key, x => x.Value);
            OnLeaderboardUpdated.Invoke();
        }
    }
}
