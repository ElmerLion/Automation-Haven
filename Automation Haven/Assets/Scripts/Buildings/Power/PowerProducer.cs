using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerProducer : MonoBehaviour {

    private PowerData powerData;
    private float timer;
    private float timerMax;
    private float storedPower;

    // Power Producer that needs resources to produce power
    private int amountStored;
    private int maxStorage = 1;
    private Inventory inventory;

    private void Start() {
        powerData = GetComponent<BuildingTypeHolder>().buildingType.powerData;
        timerMax = powerData.productionRate;

        inventory = new Inventory(maxStorage);
    }

    private void Update() {
        if (storedPower >= powerData.powerStorage) { return; }
        if (powerData.requiredItem != null && amountStored <= 0) { return; }

        timer += Time.deltaTime;
        if (timer >= timerMax) {
            timer = 0f;
            if (powerData.powerProduction > 0 && storedPower < powerData.powerStorage) {
                storedPower += powerData.powerProduction;
                if (powerData.requiredItem != null) {
                    amountStored--;
                }
            }
        }
    }


    public float GetStoredPower() {
        return storedPower;
    }

    public float TakeStoredPower(float amount) {
        float amountTaken = Mathf.Min(storedPower, amount);
        storedPower -= amountTaken;
        return amountTaken;
    }

    public void DistributePower(float amount) {
        storedPower -= amount;
        storedPower = Mathf.Max(storedPower, 0);
    }

    public List<ItemAmount> GetInventoryItems() {
        ItemAmount itemAmount = new ItemAmount(powerData.requiredItem, amountStored);
        List<ItemAmount> itemAmountList = new List<ItemAmount>() { itemAmount };
        return itemAmountList;
    }

    public int GetMaxStorage() {
        return maxStorage;
    }
    public int GetStoredAmount() {
        return amountStored;
    }

    public void AddItemToInventory() {
        amountStored++;
    }
    public ItemSO RemoveItemFromInventory() {
        amountStored--;
        return powerData.requiredItem;
    }
    public ItemSO GetRequiredItem() {
        return powerData.requiredItem;
    }
}