using UnityEngine;

public class ReplicateMovement : MonoBehaviour
{
    // References to main car and replicated objects
    public SimController simController;
    private Transform mainCar;              // The car in the main scene (moving car)
    public Transform replicatedCar;       // The car in the replicated scene (fixed car)
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
        mainCar = simController.carModels[simController.selectedDriver.driver_number].transform;
        if (mainCar != null && replicatedTrack != null && replicatedCar != null)
        {
            // Calculate the movement delta of the main car in local space
            Vector3 mainCarLocalMovementDelta = mainCar.localPosition - lastMainCarLocalPosition;

            // Move the replicated track based on the car's local movement
            replicatedTrack.localPosition -= mainCarLocalMovementDelta;

            // Update the last known local position of the main car
            lastMainCarLocalPosition = mainCar.localPosition;
        }

        // Ensure the replicated car remains fixed
        replicatedCar.localPosition = Vector3.zero; // or set any desired fixed position
        replicatedCar.localRotation = Quaternion.identity; // or any desired fixed rotation

        SyncRotation();
    }

     void SyncRotation()
    {
        if (mainCar != null && replicatedTrack != null)
        {
            // Apply rotation to the replicated car (if needed)
            replicatedCar.localRotation = mainCar.localRotation;
        }
    }
}
