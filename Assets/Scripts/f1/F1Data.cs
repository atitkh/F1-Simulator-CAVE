using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using MiddleVR;

public class F1Data : MonoBehaviour
{
    public UnityEvent OnDataFetched = new UnityEvent();
    private Dictionary<int, List<TrackData>> allTrackData = new Dictionary<int, List<TrackData>>();
    private Dictionary<int, Driver> allDrivers = new Dictionary<int, Driver>();
    private const string locationUrl = "https://api.openf1.org/v1/location?session_key={0}&driver_number={1}&date>={2}&date<{3}";
    private const string carDataUrl = "https://api.openf1.org/v1/car_data?session_key={0}&driver_number={1}&date>={2}&date<{3}";
    private const string intervalUrl = "https://api.openf1.org/v1/intervals?session_key={0}&driver_number={1}";
    private const string driversUrl = "https://api.openf1.org/v1/drivers?session_key={0}";
    private int scaleFactor = 1;
    private int maxDataPull = 1;
    private bool invoked = false;
    public TMP_Text DebugText;

    private void Update()
    {
        if (!invoked)
        {
            CheckDataFetched();
        }
    }

    private void setDebugText(string text)
    {
        DebugText.text = text;
    }

    public void GetTrackData(string sessionKey, int driverId, string startTime, string endTime)
    {
        string locationRequestUrl = string.Format(locationUrl, sessionKey, driverId, startTime, endTime);
        string carDataRequestUrl = string.Format(carDataUrl, sessionKey, driverId, startTime, endTime);
        string intervalDataRequestUrl = string.Format(intervalUrl, sessionKey, driverId);

        StartCoroutine(FetchTrackData(driverId, locationRequestUrl, carDataRequestUrl, intervalDataRequestUrl));
    }
    IEnumerator FetchTrackData(int driverId, string locationRequestUrl, string carDataRequestUrl, string intervalDataRequestUrl)
    {
        using (UnityWebRequest locationRequest = UnityWebRequest.Get(locationRequestUrl))
        using (UnityWebRequest carDataRequest = UnityWebRequest.Get(carDataRequestUrl))
        using (UnityWebRequest intervalDataRequest = UnityWebRequest.Get(intervalDataRequestUrl))
        {
            yield return locationRequest.SendWebRequest();
            yield return carDataRequest.SendWebRequest();
            yield return intervalDataRequest.SendWebRequest();

            // Check for errors in the location request
            if (locationRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Requests failed for Driver {driverId}: {locationRequest.error}, {carDataRequest.error}, {intervalDataRequest.error}");
                setDebugText($"Requests failed for Driver {driverId}: {locationRequest.error}, {carDataRequest.error}, {intervalDataRequest.error}");
            }
            else
            {
                try
                {
                    string locationJson = "{\"data\":" + locationRequest.downloadHandler.text + "}";
                    LocationData locationDatas = JsonUtility.FromJson<LocationData>(locationJson);

                    // Sort location data by date
                    locationDatas.data = locationDatas.data.OrderBy(l => DateTime.Parse(l.date)).ToList();

                    // Check for errors in the car data request
                    if (carDataRequest.result != UnityWebRequest.Result.Success || intervalDataRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Car data request failed for Driver {driverId}: {carDataRequest.error}");
                        setDebugText($"Car data request failed for Driver {driverId}: {carDataRequest.error}");
                    }
                    else
                    {
                        try
                        {
                            string carDataJson = "{\"data\":" + carDataRequest.downloadHandler.text + "}";
                            CarData carDatas = JsonUtility.FromJson<CarData>(carDataJson);

                            string intervalDataJson = "{\"data\":" + intervalDataRequest.downloadHandler.text + "}";
                            IntervalList intervalDatas = JsonUtility.FromJson<IntervalList>(intervalDataJson);

                            // Sort data by date
                            carDatas.data = carDatas.data.OrderBy(c => DateTime.Parse(c.date)).ToList();
                            //carDatas.data = carDatas.data.OrderBy(c => MVR.Kernel.GetTime().Parse(c.date)).ToList();
                            intervalDatas.data = intervalDatas.data.OrderBy(i => DateTime.Parse(i.date)).ToList();

                            // Merge location, car data and intervals data
                            List<TrackData> mergedData = MergeLCIData(locationDatas.data, carDatas.data, intervalDatas.data);
                            allTrackData[driverId] = mergedData;

                            Debug.Log($"Data fetched successfully for Driver {driverId}");
                            Debug.Log($"Merged data count for Driver {driverId}: {mergedData.Count}");
                            Debug.Log($"Total data stored: {allTrackData.Sum(d => d.Value.Count)}");
                            setDebugText($"Data fetched successfully for Driver {driverId}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error parsing car data for Driver {driverId}: {e.Message}");
                            setDebugText($"Error parsing car data for Driver {driverId}: {e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing location data for Driver {driverId}: {e.Message}");
                }
            }
        }
    }

    public void GetAllDrivers(string sessionKey, string startTime, string endTime)
    {
        string driversRequestUrl = string.Format(driversUrl, sessionKey);
        StartCoroutine(FetchAllDrivers(sessionKey, driversRequestUrl, startTime, endTime));
    }

    IEnumerator FetchAllDrivers(string sessionKey, string driversRequestUrl, string startTime, string endTime)
    {
        using (UnityWebRequest driversRequest = UnityWebRequest.Get(driversRequestUrl))
        {
            yield return driversRequest.SendWebRequest();

            if (driversRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Drivers request failed: {driversRequest.error}");
                setDebugText($"Drivers request failed: {driversRequest.error}");
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(driversRequest.downloadHandler.text))
                    {
                        string driversJson = "{\"data\":" + driversRequest.downloadHandler.text + "}";

                        DriverList drivers = JsonUtility.FromJson<DriverList>(driversJson);

                        // Debug.Log($"Drivers fetched successfully for Session {sessionKey} {drivers.data.Count}");
                        allDrivers = drivers.data.ToDictionary(driver => driver.driver_number, driver => driver);
                        Debug.Log($"{allDrivers.Count} Drivers fetched successfully for Session {sessionKey}");

                        int count = 0;
                        foreach (var driver in allDrivers)
                        {
                            if (count < maxDataPull)
                            {
                                Debug.Log($"Pulling data for Driver {driver.Key}: {driver.Value.full_name}");
                                GetTrackData(sessionKey, driver.Key, startTime, endTime);
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Error parsing drivers data: Download handler text is null or empty");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing drivers data: {e.Message}");
                    setDebugText($"Error parsing drivers data: {e.Message}");
                }
            }
        }
    }

    private void CheckDataFetched()
    {
        Debug.Log($"All Track Data Count: {allTrackData.Count}");
        Debug.Log($"All Drivers Count: {allDrivers.Count}");
        if (allDrivers.Count > 0 && allTrackData.Count == maxDataPull)
        {
            setDebugText("All data fetched successfully");
            OnDataFetched.Invoke();
            invoked = true;
        }
    }

    // Merge location and car data using a sliding window approach
    private List<TrackData> MergeLCIData(List<Location> locationData, List<Car> carData, List<Interval> intervalData)
    {
        List<TrackData> mergedData = new List<TrackData>();
        int carDataIndex = 0;
        int intervalDataIndex = 0;

        foreach (Location location in locationData)
        {
            DateTime locationDate = DateTime.Parse(location.date);

            // Find the closest car data using binary search
            int closestIndex = FindClosestCarDataIndex(carData, locationDate, carDataIndex);
            Car closestCarData = carData[closestIndex];

            // Find the closest interval data using binary search
            int closestIntervalIndex = FindClosestIntervalDataIndex(intervalData, locationDate, intervalDataIndex);
            Interval closestIntervalData = intervalData[closestIntervalIndex];

            // Update the carDataIndex to optimize future searches
            carDataIndex = closestIndex;
            intervalDataIndex = closestIntervalIndex;

            mergedData.Add(new TrackData
            {
                x = location.x / scaleFactor,
                y = location.y / scaleFactor,
                z = location.z / scaleFactor,
                date = location.date,
                carData = closestCarData,
                intervalData = closestIntervalData
            });
        }

        return mergedData;
    }

    private int FindClosestCarDataIndex(List<Car> carData, DateTime targetDate, int startIndex)
    {
        int left = startIndex;
        int right = carData.Count - 1;
        int closestIndex = left;
        double minTimeDiff = double.MaxValue;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DateTime carDate = DateTime.Parse(carData[mid].date);
            double timeDiff = Math.Abs((carDate - targetDate).TotalMilliseconds);

            if (timeDiff < minTimeDiff)
            {
                minTimeDiff = timeDiff;
                closestIndex = mid;
            }

            if (carDate < targetDate)
            {
                left = mid + 1;
            }
            else if (carDate > targetDate)
            {
                right = mid - 1;
            }
            else
            {
                // Exact match
                return mid;
            }
        }

        return closestIndex;
    }

    private int FindClosestIntervalDataIndex(List<Interval> intervalData, DateTime targetDate, int startIndex)
    {
        int left = startIndex;
        int right = intervalData.Count - 1;
        int closestIndex = left;
        double minTimeDiff = double.MaxValue;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            DateTime intervalDate = DateTime.Parse(intervalData[mid].date);
            double timeDiff = Math.Abs((intervalDate - targetDate).TotalMilliseconds);

            if (timeDiff < minTimeDiff)
            {
                minTimeDiff = timeDiff;
                closestIndex = mid;
            }

            if (intervalDate < targetDate)
            {
                left = mid + 1;
            }
            else if (intervalDate > targetDate)
            {
                right = mid - 1;
            }
            else
            {
                // Exact match
                return mid;
            }
        }
        return closestIndex;
    }

    private void OnApplicationQuit()
    {
        allTrackData.Clear();
        allDrivers.Clear();
    }

    public List<TrackData> GetAllTrackData(int driverId)
    {
        return allTrackData.ContainsKey(driverId) ? allTrackData[driverId] : null;
    }

    public Driver GetDriver(int driverId)
    {
        return allDrivers.ContainsKey(driverId) ? allDrivers[driverId] : null;
    }

    public List<Driver> AllDrivers()
    {
        return allDrivers.Values.ToList();
    }
}