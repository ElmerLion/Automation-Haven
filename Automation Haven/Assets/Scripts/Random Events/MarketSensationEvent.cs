using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketSensationEvent : Event {

    [SerializeField] private List<ItemSO> possibleItems;
    [SerializeField] private int minHoursUntilFall;
    [SerializeField] private int maxHoursUntilFall;

    public override bool TryTriggerEvent() {
        ItemSO item = possibleItems[Random.Range(0, possibleItems.Count)];

        MarketManager.Instance.TriggerMarketSensation(item, Random.Range(minHoursUntilFall, maxHoursUntilFall), out string message);

        string newEventDescription = StringUtility.ReplacePlaceholders(eventDescription, item);

        newEventDescription += "\n\n" + message;

        MessageBarUI.Instance.CreateMessage("Market Sensation", newEventDescription, eventType);

        return base.TryTriggerEvent();
    }

}
