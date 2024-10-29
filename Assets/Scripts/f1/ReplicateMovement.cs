using UnityEngine;

public class ReplicateMovement : MonoBehaviour
{
    // References to main car and replicated objects
    public SimController simController;
    public GameObject carParent;
    private Transform mainCar;              // The car in the main scene (moving car)
    public GameObject replicatedCar;       // The car in the replicated scene (fixed car)
    private Transform replicatedCarTransform;       // The car in the replicated scene (fixed car)
    public Transform replicatedTrack;     // The track in the replicated scene (moving track)

    // For tracking the car’s last local position
    private Vector3 lastMainCarLocalPosition;

    void Start()
    {
        // Initialize the last known local position of the main car
        if (mainCar != null)
        {
            lastMainCarLocalPosition = mainCar.localPosition;
        }
    }

    void Update()
    {
        ReloadCar();
        if(simController.carModels.Count == 0)
        {
            return;
        }
        mainCar = simController.carModels[simController.selectedDriver.driver_number].transform;
        if (mainCar != null && replicatedTrack != null && replicatedCarTransform != null)
        {
            // Calculate the movement delta of the main car in local space
            Vector3 mainCarLocalMovementDelta = mainCar.localPosition - lastMainCarLocalPosition;

            // Move the replicated track based on the car's local movement
            replicatedTrack.localPosition -= mainCarLocalMovementDelta;

            // Update the last known local position of the main car
            lastMainCarLocalPosition = mainCar.localPosition;
        }

        // Ensure the replicated car remains fixed
        replicatedCarTransform.localPosition = Vector3.zero; // or set any desired fixed position
        replicatedCarTransform.localRotation = Quaternion.identity; // or any desired fixed rotation

        SyncRotation();
    }

    void ReloadCar()
    {
        // // Set the driver acronym
        if(replicatedCar == null)
        {
            replicatedCar = Instantiate(simController.carPrefab, Vector3.zero, Quaternion.identity, carParent.transform);
        }
        replicatedCarTransform = replicatedCar.transform;
        replicatedCar.GetComponent<CarController>().DriverAcronym = simController.selectedDriver.name_acronym;
        replicatedCar.GetComponent<CarController>().simController = this.simController;
        replicatedCar.GetComponent<CarController>().driver = simController.selectedDriver;
    }

     void SyncRotation()
    {
        if (mainCar != null && replicatedTrack != null)
        {
            // Apply rotation to the replicated car (if needed)
            replicatedCarTransform.localRotation = mainCar.localRotation;

            // Apply rotation to the replicated Car's wheels (if needed)
            // replicatedCar.GetComponent<CarController>().TurnWheel(replicatedCar.GetComponent<CarController>().frontLeftWheel, mainCar.localRotation.eulerAngles.y);
            // replicatedCar.GetComponent<CarController>().TurnWheel(replicatedCar.GetComponent<CarController>().frontRightWheel, mainCar.localRotation.eulerAngles.y);
        }
    }
}
