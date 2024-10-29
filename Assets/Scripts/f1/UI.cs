using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MiddleVR;

public class UI : MonoBehaviour
{
    public SimController simController;
    public TMP_Text timeText;
    public TMP_Text speedText;
    public TMP_Dropdown driverDropdown;
    public Slider speedSlider;
    public GameObject menuPanel;
    public GameObject startSimulationButton;
    // [SerializeField] private OVRInput.Controller controller;
    private bool readyToStart = false;

    private void Start()
    {
        simController.F1Data.OnDataFetched.AddListener(() =>
        {
            readyToStart = true;
            populateDriverDropdown();
            startSimulationButton.SetActive(true);
        });
    }

    private void Update()
    {
        if (!readyToStart)
        {
            timeText.text = "Loading data...";
            timeText.enabled = true;
        }
        else
        {
            timeText.enabled = true;
            timeText.text = simController.simulatedTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        if (simController.timeMultiplier != 1)
        {
            speedText.text = "Speed: " + simController.timeMultiplier + "x";
        }
        else
        {
            speedText.text = "Speed: Normal";
        }

        // if (OVRInput.GetDown(OVRInput.Button.One, controller))
        // {
        //     Debug.Log("Button " + OVRInput.Button.One + " pressed");
        //     ToggleMenu();
        // }

        // if (MVR.DeviceMgr.IsWandButtonToggled(0, MVR.WandButton.WandButton1))
        //     Debug.Log("Button 0 pressed");
        //     ToggleMenu();
        // }
    }

    private void populateDriverDropdown()
    {
        driverDropdown.ClearOptions();
        var driverOptions = simController.F1Data.AllDrivers();
        var driversToRemove = new List<Driver>();
        driverOptions.ForEach(driver =>
        {
            if (!simController.F1Data.HasDriverData(driver.driver_number))
            {
                driversToRemove.Add(driver);
            }
        });
        driversToRemove.ForEach(driver => driverOptions.Remove(driver));
        driverDropdown.AddOptions(driverOptions.ConvertAll(d => new TMP_Dropdown.OptionData(d.full_name)));
        driverDropdown.value = 0;
    }

    public void OnDropdownValueChanged()
    {
        int pickedDriver = driverDropdown.value;
        string driverName = driverDropdown.options[pickedDriver].text;
        Driver selectedDriver = simController.F1Data.GetDriverByName(driverName);
        simController.selectDriver(selectedDriver.driver_number);
    }

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void StartSimulation()
    {
        if (readyToStart)
        {
            simController.timeMultiplier = 1;
            simController.StartSimulation();
            startSimulationButton.SetActive(false);
        }
    }

    public void StopSimulation()
    {
        simController.StopSimulation();
    }

    public void ResetSimulation()
    {
        simController.ResetSimulation();
    }

    public void PausePlaySimulation()
    {
        simController.PausePlaySimulation();
    }

    public void IncreaseSpeed()
    {
        simController.timeMultiplier++;
    }

    public void DecreaseSpeed()
    {
        simController.timeMultiplier--;
    }

    public void SetSpeedFromSlider()
    {
        simController.timeMultiplier = (int)speedSlider.value;
    }
}