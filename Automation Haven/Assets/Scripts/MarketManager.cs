using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MarketManager : MonoBehaviour {

    public static MarketManager Instance { get; private set; }

    private Dictionary<ItemSO, int> marketItemPriceList;

    private void Awake() {
        Instance = this;
    }


    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;

        TimeManager.Instance.OnDayChanged += OnDayChanged;
        TimeManager.Instance.OnWeekChanged += OnWeekChanged;
        ContractManager.Instance.OnContractCompleted += OnContractCompleted;
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
    }

    private void OnWeekChanged(object sender, System.EventArgs e) {
        UpdateItemsBelowBasePrice();
    }

    private void OnContractCompleted(ContractManager.Contract contract) {
        foreach (ItemAmount itemAmount in contract.neededItemAmount) {
            DecreaseMarketPriceForItem(itemAmount);
        }
    }

    private void OnDayChanged(object sender, System.EventArgs e) {
        RandomizeMarketChanges();
    }

    private void DecreaseMarketPriceForItem(ItemAmount itemAmount) {
        ItemSO itemSO = itemAmount.itemSO;
        int price = marketItemPriceList[itemSO];
        price -= (Random.Range(itemSO.price, itemSO.price + 10) / 25) * itemAmount.amount;
        price = Mathf.Clamp(price, itemSO.price / 2, itemSO.price * 2);
        marketItemPriceList[itemSO] = price;

        Debug.Log("Market price for " + itemSO.nameString + " is now " + price + " credits.");
    }
    
    private void RandomizeMarketChanges() {
        List<ItemSO> marketPriceKeys = new List<ItemSO>(marketItemPriceList.Keys);

        MessageBarUI.Instance.CreateMessage("Market Price Changes", "The market prices have changed.", MessageBarUI.MessageType.Neutral);

        foreach (ItemSO itemSO in marketPriceKeys) {
            int price = marketItemPriceList[itemSO];
            price += Random.Range(-itemSO.price / 10, itemSO.price / 10);
            price = Mathf.Clamp(price, itemSO.price / 2, itemSO.price * 2);

            if (price < itemSO.price) {
                //MessageBarUI.Instance.CreateMessage("Market Price Change", "The market price for " + itemSO.nameString + " has decreased to " + price + " credits. \nFormer Price: " + marketItemPricesDic[itemSO], MessageBarUI.MessageType.Negative);
            }

            if (price > itemSO.price) {
                //MessageBarUI.Instance.CreateMessage("Market Price Change", "The market price for " + itemSO.nameString + " has increased to " + price + " credits. \nFormer Price: " + marketItemPricesDic[itemSO], MessageBarUI.MessageType.Positive);
            }

            marketItemPriceList[itemSO] = price;

            //Debug.Log("Market price for " + itemSO.nameString + " is now " + price + " credits.");
        }
    }

    private void UpdateItemsBelowBasePrice() {
        List<ItemSO> marketPriceKeys = new List<ItemSO>(marketItemPriceList.Keys);

        foreach (ItemSO itemSO in marketPriceKeys) {
            if (marketItemPriceList[itemSO] < itemSO.price) {
                marketItemPriceList[itemSO] += Random.Range(1, 3);
            }
        }
    }

    public int GetItemPrice(ItemSO itemSO) {
        foreach (ItemSO item in marketItemPriceList.Keys) {
            Debug.Log("Item In Market: " + item.nameString);
        }
        return marketItemPriceList[itemSO];
    }

}
