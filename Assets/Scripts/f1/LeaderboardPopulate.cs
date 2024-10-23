using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPopulate : MonoBehaviour
{
    public SimController simController;
    public GameObject leaderboardEntryPrefab;
    public List<GameObject> leaderboardEntriesList;

    // Start is called before the first frame update
    void Start()
    {
        simController.OnLeaderboardUpdated.AddListener(PopulateLeaderboard);
    }

    // Update is called once per frame
    void Update()
    {
        if (simController.isSimulating)
        {
            UpdateLeaderboardTimer();
        }
    }

    void PopulateLeaderboard()
    {
        // foreach (Transform child in transform)
        // {
        //     if (child.gameObject.name == "Background" || child.gameObject.name == "Logos" || child.gameObject.name == "Countdown") continue;
        //     Destroy(child.gameObject);
        // }

        if (simController != null && leaderboardEntryPrefab != null)
        {
            foreach (KeyValuePair<int, Interval> entry in simController.leaderboard)
            {
                Driver driver = simController.F1Data.GetDriver(entry.Value.driver_number);
                GameObject leaderboardEntry;
                // Check if the entry is already present in leaderboardEntries
                if (leaderboardEntriesList.Exists(x => x.name == entry.Value.driver_number.ToString()))
                {
                    leaderboardEntry = leaderboardEntriesList.Find(x => x.name == entry.Value.driver_number.ToString());
                }
                else
                {
                    leaderboardEntry = Instantiate(leaderboardEntryPrefab, Vector3.zero, Quaternion.identity, transform);
                    leaderboardEntry.name = entry.Value.driver_number.ToString();
                    leaderboardEntriesList.Add(leaderboardEntry);
                }

                leaderboardEntry.transform.Find("Name").GetComponent<TMP_Text>().text = driver.name_acronym;
                leaderboardEntry.transform.Find("Position").GetComponent<TMP_Text>().text = entry.Value.position.ToString();

                if (entry.Value.gap_to_leader <= 0)
                {
                    leaderboardEntry.transform.Find("Gap").GetComponent<TMP_Text>().text = "Int";
                }
                else
                {
                    leaderboardEntry.transform.Find("Gap").GetComponent<TMP_Text>().text = $"+{entry.Value.gap_to_leader.ToString()}";
                }

                leaderboardEntry.transform.localPosition = new Vector3(0, 0, 7000 - (750 * (float)entry.Value.position));
                leaderboardEntry.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void UpdateLeaderboardTimer()
    {
        TimeSpan totalRaceTime = DateTime.Parse(simController.endTime) - DateTime.Parse(simController.startTime);
        TimeSpan remainingTime = DateTime.Parse(simController.endTime) - simController.simulatedTime;

        transform.Find("Countdown").Find("Time").GetComponent<TMP_Text>().text = remainingTime.ToString(@"mm\:ss");
    }
}
