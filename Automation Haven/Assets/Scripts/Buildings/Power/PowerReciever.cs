using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerReciever : MonoBehaviour {

    public static event EventHandler OnPowerNeeded;
    public static event EventHandler OnPowerAvailable;
    public event EventHandler OnPowerStatusChanged;

    private PlacedObjectTypeSO placedObjectTypeSO;
    private float powerConsumption;
    private float availablePower;
    private float powerConsumptionMultiplier;

    private void Start() {
        placedObjectTypeSO = transform.GetComponent<BuildingTypeHolder>().buildingType;
        powerConsumption = placedObjectTypeSO.powerData.powerUsage;

        OnPowerNeeded?.Invoke(this, EventArgs.Empty);
        OnPowerStatusChanged?.Invoke(this, EventArgs.Empty);

        InvokeRepeating("CheckPowerStatus", 1f, 1.5f);
    }

    private void CheckPowerStatus() {
        OnPowerNeeded?.Invoke(this, EventArgs.Empty);
    }

    private void CalculatePowerConsumption() {
        powerConsumption = powerConsumption * (1 - powerConsumptionMultiplier);
    }

    public float GetPowerConsumption() {
        CalculatePowerConsumption();
        return powerConsumption;
    }

    public void AddPowerConsumption(float amount) {
        powerConsumption += amount;
        CalculatePowerConsumption();
    }

    public void AddPowerConsumptionMultiplier(float multiplier) {
        powerConsumptionMultiplier += multiplier;
        CalculatePowerConsumption();
    }

    public void AddAvailablePower(float amount) {
        availablePower += amount;
        OnPowerAvailable?.Invoke(this, EventArgs.Empty);
        OnPowerStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPowerAvailable() {
        return availablePower >= powerConsumption;
    }

    public void ConsumePower() {
        availablePower -= powerConsumption;
        OnPowerNeeded?.Invoke(this, EventArgs.Empty);
        OnPowerStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPowerNeeded() {
        return availablePower < powerConsumption;
    }

}
