using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEconomyManager : MonoBehaviour {

    public static PlayerEconomyManager Instance { get; private set; }

    [SerializeField] private int currentMoney;
    [SerializeField] private int startingMoney = 1000;

    private void Awake() {
        Instance = this;

        currentMoney = startingMoney;
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        ContractManager.Instance.OnContractCompleted += ContractManager_OnContractCompleted;
    }

    private void ContractManager_OnContractCompleted(ContractManager.Contract obj) {
        AddMoney(obj.reward);
    }

    public void AddMoneyForItem(ItemObject itemObject) {
        AddMoney(MarketManager.Instance.GetItemPrice(itemObject.GetItemSO()));
    }

    public void AddMoney(int amount) {
        currentMoney += amount;
    }

    public bool TryRemoveMoney(int amount) {
        if (currentMoney - amount >= 0) {
            currentMoney -= amount;
            return true;
        }
        return false;
    }

    public bool CanAfford(int amount) {
        return currentMoney - amount >= 0;
    }

    private void SaveManager_OnGameSaved(string filePath) {
        ES3.Save("currentMoney", currentMoney, filePath);
    }

    private void SaveManager_OnGameLoaded(string filePath) {
        currentMoney = ES3.Load("currentMoney", filePath, startingMoney);
    }

}
