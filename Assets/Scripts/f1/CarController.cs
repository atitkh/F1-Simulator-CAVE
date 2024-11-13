using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiddleVR;

public class CarController : MonoBehaviour
{
    public SimController simController;
    public GameObject nameTag;
    public GameObject driverDetails;
    [HideInInspector]
    public string DriverAcronym;
    public bool isReplica = false;
    public Driver driver;
    private TrackData trackData;

    // wheels for rotation and turning
    public GameObject frontLeftWheel;
    public GameObject frontRightWheel;
    public GameObject rearLeftWheel;
    public GameObject rearRightWheel;

    private float wheelSpeed = 0f;
    private float turnAngle = 0f;

    public Texture2D barTexture; // select some white or gray texture
    public float barWidth = 5;
    public float barHeight = 40;
    public float barSpace = 7;

    // Store the car's previous rotation
    private Quaternion previousRotation;

    void Start()
    {
        nameTag.GetComponentInChildren<TMP_Text>().text = DriverAcronym;

        // Initialize previous rotation
        previousRotation = transform.rotation;

        if (isReplica)
        {
            simController.OnTelemetryUpdated.AddListener(OnTelemetryUpdated);
            // driverDetails.SetActive(true);
            nameTag.SetActive(false);
        }
    }

    void Update()
    {
        // if (isReplica && simController.isSimulating)
        // {
        //     OnTelemetryUpdated();
        // }
        // CalculateWheelTurn();
    }

    public void OnTelemetryUpdated()
    {
        if (simController.selectedDriver == null || simController.selectedDriverTrackData == null)
        {
            return;
        }

        if (simController.selectedDriver.driver_number == driver.driver_number)
        {
            if (driverDetails.activeSelf == false)
            {
                driverDetails.SetActive(true);
            }
            trackData = simController.selectedDriverTrackData;

            var telemetryPanelUpdater = driverDetails.GetComponentInChildren<TelemetryPanelUpdater>();
            telemetryPanelUpdater.trackData = trackData;
            telemetryPanelUpdater.driver = driver;

            if (simController.isSimulating)
            {
                SetWheelSpeed(trackData.carData.speed);
            }
        }
    }

    // private void OnGUI() {
    //     if (isReplica && simController.isSimulating && trackData != null)
    //     {
    //         DisplayBar(trackData.carData.throttle);
    //     }
    // }

    void DisplayBar(int speed)
    {
        Vector2 pos = new Vector2(Screen.width - 120, Screen.height - 50);
        for (float v = 1; v <= speed; v += 10)
        {
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(pos.x, pos.y, barWidth, barHeight), barTexture); // draw a single bar
            pos.x += barSpace; // next bar at barSpace pixels to the right
        }
    }

    public void SpinWheel(GameObject wheel, float speed)
    {
        float speedInMetersPerSecond = speed * 1000f / 3600f; // Convert km/h to m/s
        float wheelRadius = 0.33f;  // Adjust the radius based on your wheel model
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;

        float rotationSpeed = speedInMetersPerSecond / wheelCircumference;
        float degreesPerSecond = rotationSpeed * Mathf.Rad2Deg;

        // Rotate the wheel around its local right axis for spinning effect
        wheel.transform.Rotate(Vector3.up, (float)(degreesPerSecond * MVR.Kernel.GetDeltaTime()));
    }

    // Method to calculate dynamic wheel turning based on car rotation
    public void CalculateWheelTurn()
    {
        // Get the current car rotation
        Quaternion currentRotation = transform.rotation;

        // Calculate the difference in rotation (yaw)
        float rotationDelta = Quaternion.Angle(previousRotation, currentRotation);

        // Check if there is a significant change in yaw to apply turning
        if (rotationDelta > 0.1f)
        {
            Vector3 deltaEuler = (currentRotation.eulerAngles - previousRotation.eulerAngles);
            float yawDelta = Mathf.Clamp(deltaEuler.y, -30f, 30f); // limit the wheel turn angle
            SetWheelTurn(yawDelta);
        }

        // Update previous rotation for the next frame
        previousRotation = currentRotation;
    }

    // Quaternion-based wheel turning
    public void TurnWheel(GameObject wheel, float angle)
    {
        Quaternion localRotation = Quaternion.Euler(0, angle, 0);
        wheel.transform.localRotation = localRotation * wheel.transform.localRotation; // Apply turn
    }

    public void SetWheelSpeed(float speed)
    {
        wheelSpeed = speed;
        SpinWheel(frontLeftWheel, -1 * wheelSpeed);
        SpinWheel(frontRightWheel, wheelSpeed);
        SpinWheel(rearLeftWheel, -1 * wheelSpeed);
        SpinWheel(rearRightWheel, wheelSpeed);
    }

    public void SetWheelTurn(float angle)
    {
        turnAngle = angle;
        TurnWheel(frontLeftWheel, turnAngle);
        TurnWheel(frontRightWheel, turnAngle);
    }
}
