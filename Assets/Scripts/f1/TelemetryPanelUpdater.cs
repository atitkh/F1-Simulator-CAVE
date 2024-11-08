using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class TelemetryPanelUpdater : MonoBehaviour
{
    public TMP_Text FName;
    public TMP_Text LName;
    public TMP_Text MoreDetails;
    public GameObject Headshot;
    public GameObject RpmBar;
    public GameObject BrakeBar;
    public GameObject ThrottleBar;
    public TMP_Text GearText;
    public Driver driver;
    public TrackData trackData;

    private string headshotUrl = "";
    private Vector3 OriginalRpmBarScale;
    private Vector3 OriginalBrakeBarScale;
    private Vector3 OriginalThrottleBarScale;
    private Vector3 OriginalRpmBarPosition;
    private Vector3 OriginalBrakeBarPosition;
    private Vector3 OriginalThrottleBarPosition;

    // Start is called before the first frame update
    void Start()
    {
        OriginalRpmBarScale = RpmBar.transform.localScale;
        OriginalBrakeBarScale = BrakeBar.transform.localScale;
        OriginalThrottleBarScale = ThrottleBar.transform.localScale;
        OriginalRpmBarPosition = RpmBar.transform.localPosition;
        OriginalBrakeBarPosition = BrakeBar.transform.localPosition;
        OriginalThrottleBarPosition = ThrottleBar.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {   
        if (headshotUrl != driver.headshot_url)
        {
            headshotUrl = driver.headshot_url;
            StartCoroutine(LoadHeadshot(headshotUrl));
        }
        // Update UI
        FName.text = driver.first_name;
        LName.text = driver.last_name;
        MoreDetails.text = driver.driver_number.ToString() + " | " + driver.name_acronym + " | " + driver.team_name;

        Color teamColor;
        if (ColorUtility.TryParseHtmlString(driver.team_colour, out teamColor))
        {
            FName.color = teamColor;
            LName.color = teamColor;
            MoreDetails.color = teamColor;
        }
        GearText.text = trackData.carData.n_gear.ToString();
        float throttleScale = trackData.carData.throttle / 100f;
        float brakeScale = trackData.carData.brake / 100f;
        float rpmScale = trackData.carData.rpm / 15000f;

        ThrottleBar.transform.localScale = new Vector3(ThrottleBar.transform.localScale.x, ThrottleBar.transform.localScale.y, throttleScale);
        ThrottleBar.transform.localPosition = new Vector3(ThrottleBar.transform.localPosition.x, ThrottleBar.transform.localPosition.y, Mathf.Round(OriginalThrottleBarPosition.x) + (-0.5f * (1 - throttleScale)));

        BrakeBar.transform.localScale = new Vector3(BrakeBar.transform.localScale.x, BrakeBar.transform.localScale.y, brakeScale);
        BrakeBar.transform.localPosition = new Vector3(BrakeBar.transform.localPosition.x, BrakeBar.transform.localPosition.y, Mathf.Round(OriginalBrakeBarPosition.z) + (-0.5f * (1 - brakeScale)));

        RpmBar.transform.localScale = new Vector3(rpmScale, RpmBar.transform.localScale.y, RpmBar.transform.localScale.z);
        RpmBar.transform.localPosition = new Vector3(Mathf.Round(OriginalRpmBarPosition.x) + (-0.5f * (1 - rpmScale)), RpmBar.transform.localPosition.y, RpmBar.transform.localPosition.z);
    }

    IEnumerator LoadHeadshot(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Headshot.GetComponent<Renderer>().material.mainTexture = texture;
        }
    }
}
