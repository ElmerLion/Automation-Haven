using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewHousesEvent : Event {

    public override bool TryTriggerEvent() {
        List<ItemSO> furnitureItems = ItemManager.Instance.GetItemsInCategory(ItemSO.ItemCategory.Furniture);

        foreach (ItemSO item in furnitureItems) {
            MarketManager.Instance.TriggerPriceFluctuationForItem(item, Random.Range(item.price / 5, item.price / 3), false, out string message, out EventType eventType);
        }

        MessageBarUI.Instance.CreateMessage(eventName, eventDescription, EventType.Neutral);

        return base.TryTriggerEvent();
    }

}
