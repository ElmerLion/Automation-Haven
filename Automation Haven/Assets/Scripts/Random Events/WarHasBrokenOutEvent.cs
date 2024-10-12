using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarHasBrokenOutEvent : Event {

    public override bool TryTriggerEvent() {
        List<ItemSO> warItems = ItemManager.Instance.GetItemsInCategory(ItemSO.ItemCategory.Weapons);

        foreach (ItemSO item in warItems) {
            MarketManager.Instance.TriggerPriceFluctuationForItem(item, Random.Range(item.price / 5, item.price / 3), false, out string message, out EventType eventType);
        }

        MessageBarUI.Instance.CreateMessage(eventName, eventDescription, EventType.Neutral);

        return base.TryTriggerEvent();
    }

}
