using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using MiddleVR;

public class CameraController : MonoBehaviour
{
    public SimController simController;
    public GameObject MVRManager;
    public GameObject DefaultCamera;

    private void Update()
    {        
        // if key pressed is 'c' then follow the selected driver
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeCam("follow");
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            ChangeCam("default");
        }
        else if(Input.GetKeyDown(KeyCode.B))
        {
            ChangeCam("cockpit");
        }
    }

    public void ChangeCam(string mode)
    {
        if (mode == "default")
        {
            var currentCar = simController.carModels[simController.selectedDriver.driver_number];
            var camera = currentCar.transform.GetChild(0).Find("CockpitCam").gameObject;
            camera.SetActive(false);
            camera = currentCar.transform.GetChild(0).Find("FollowCam").gameObject;
            camera.SetActive(false);
            SetDefaultCamera();
        }
        else if (mode == "cockpit")
        {
            var currentCar = simController.carModels[simController.selectedDriver.driver_number];
            var camera = currentCar.transform.GetChild(0).Find("CockpitCam").gameObject;

            if (MVRManager.transform.parent == camera.transform)
            {
                // Reset to main camera
                camera.SetActive(false);
                SetDefaultCamera();
            }
            else
            {
                camera.SetActive(true);
                MVRManager.transform.parent = camera.transform;
            }
        }
        else if (mode == "follow")
        {
            var currentCar = simController.carModels[simController.selectedDriver.driver_number];
            var camera = currentCar.transform.GetChild(0).Find("FollowCam").gameObject;

            if (MVRManager.transform.parent == camera.transform)
            {
                // Reset to main camera
                camera.SetActive(false);
                SetDefaultCamera();
            }
            else
            {
                camera.SetActive(true);
                MVRManager.transform.parent = camera.transform;
            }
        }
    }

    private void SetDefaultCamera()
    {
        DefaultCamera.SetActive(true);
        DefaultCamera.GetComponent<Camera>().enabled = true;
        DefaultCamera.transform.localPosition = Vector3.zero;
        DefaultCamera.transform.localRotation = Quaternion.identity;
        
        MVRManager.SetActive(false);
        MVRManager.SetActive(true);
        MVRManager.transform.parent = DefaultCamera.transform;
        // MVRManager.transform.localPosition = Vector3.zero;
        MVRManager.transform.localPosition.Set(0, -1.6f, 0);
        MVRManager.transform.localRotation = Quaternion.identity;
    }
}