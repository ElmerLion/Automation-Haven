using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MarketManager : MonoBehaviour {

    public static MarketManager Instance { get; private set; }

    private Dictionary<ItemSO, int> marketItemPriceList;

    private ItemSO marketSensationItemSO;
    private float marketSensationTimer;
    private float marketSensationTimerMax;

    private void Awake() {
        Instance = this;
    }


    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("marketPricesDic", marketItemPriceList, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        marketItemPriceList = ES3.Load("marketPricesDic", new Dictionary<ItemSO, int>());

        if (marketItemPriceList.Count != ItemManager.Instance.GetAllItemsInGame().Count) {
            foreach (ItemSO itemSO in ItemManager.Instance.GetAllItemsInGame()) {
                if (!marketItemPriceList.ContainsKey(itemSO)) {
                    marketItemPriceList.Add(itemSO, itemSO.price);
                }
            }
        }

        TimeManager.Instance.OnMonthChanged += OnMonthChanged;
        ContractManager.Instance.OnContractCompleted += OnContractCompleted;
        ContractManager.Instance.OnContractFailed += ContractManager_OnContractFailed;
    }

    private void ContractManager_OnContractFailed(ContractManager.Contract obj) {
        foreach (ItemAmount itemAmount in obj.neededItemAmount) {
            DecreaseMarketPriceForItem(itemAmount);
        }
    }

    private void OnMonthChanged(object sender, System.EventArgs e) {
        UpdateItemsNotOnBasePrice();
    }

    private void OnContractCompleted(ContractManager.Contract contract) {
        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            IncreaseMarketPriceForItem(itemAmount);
        }
    }

    private void DecreaseMarketPriceForItem(ItemAmount itemAmount) {
        ItemSO itemSO = itemAmount.itemSO;
        int price = marketItemPriceList[itemSO];
        price -= (int)CalculatePriceChangeBasedOnAmount(itemAmount);
        price = Mathf.Clamp(price, itemSO.price / 2, itemSO.price * 2);
        marketItemPriceList[itemSO] = price;

        Debug.Log("Market price for " + itemSO.nameString + " is now " + price + " credits.");
    }

    private void IncreaseMarketPriceForItem(ItemAmount itemAmount) {
        ItemSO itemSO = itemAmount.itemSO;
        int price = marketItemPriceList[itemSO];
        price += (int)CalculatePriceChangeBasedOnAmount(itemAmount);
        price = Mathf.Clamp(price, itemSO.price / 2, itemSO.price * 2);
        marketItemPriceList[itemSO] = price;

        Debug.Log("Market price for " + itemSO.nameString + " is now " + price + " credits.");
    }
   

    public void TriggerPriceFluctuationForItem(ItemSO itemSO, float fluctuationAmount, bool sendMessage, out string message, out EventType eventType) {
        message = "";
        eventType = EventType.Neutral;
        if (!marketItemPriceList.ContainsKey(itemSO)) return;

        float price = marketItemPriceList[itemSO];
        float previousPrice = price;
        price += fluctuationAmount;
        price = Mathf.Clamp(price, itemSO.price / 2, itemSO.price * 2);
        marketItemPriceList[itemSO] = (int)price;

        // Display the message
        message = fluctuationAmount > 0 ?
            $"The market price for {itemSO.nameString} has increased to {price} credits from {previousPrice} credits." :
            $"The market price for {itemSO.nameString} has decreased to {price} credits from {previousPrice} credits.";
        eventType = fluctuationAmount > 0 ? EventType.Positive : EventType.Negative;

        Debug.Log(message);

        if (sendMessage) {
            MessageBarUI.Instance.CreateMessage("Market Price Change", message, eventType);
        }
    }

    private float CalculatePriceChangeBasedOnAmount(ItemAmount itemAmount) {
        return Random.Range(itemAmount.itemSO.price / 25, itemAmount.itemSO.price / 15) * itemAmount.amount;
    }

    private void UpdateItemsNotOnBasePrice() {
        List<ItemSO> marketPriceKeys = new List<ItemSO>(marketItemPriceList.Keys);

        foreach (ItemSO itemSO in marketPriceKeys) {
            if (marketItemPriceList[itemSO] < itemSO.price) {
                marketItemPriceList[itemSO] += Random.Range(itemSO.price / 10, itemSO.price / 5);
            }

            if (marketItemPriceList[itemSO] > itemSO.price) {
                marketItemPriceList[itemSO] -= Random.Range(itemSO.price / 10, itemSO.price / 5);
            }
        }
    }

    public int GetItemPrice(ItemSO itemSO) {
        return marketItemPriceList[itemSO];
    }

    public void TriggerMarketSensation(ItemSO itemSO, int hoursUntilFall, out string priceMessage) {
        marketSensationItemSO = itemSO;

        TriggerPriceFluctuationForItem(itemSO, Random.Range(itemSO.price, itemSO.price / 2), false, out string message, out EventType eventType);
        priceMessage = message;

        marketSensationTimer = hoursUntilFall;
        TimeManager.Instance.OnHourChanged += TimeManager_OnHourChanged;
    }

    private void TimeManager_OnHourChanged() {
        marketSensationTimer--;

        if (marketSensationTimer <= 0) {
            TimeManager.Instance.OnHourChanged -= TimeManager_OnHourChanged;
            TriggerPriceFluctuationForItem(marketSensationItemSO, Random.Range(-marketSensationItemSO.price / 2, -marketSensationItemSO.price / 4), true, out string message, out EventType eventType);
        }
    }
}
