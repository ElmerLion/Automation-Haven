using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour {

    [SerializeField] private int eventChancePerHour = 50;
    [SerializeField] private List<Event> events;
    

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
    }

    private void SaveManager_OnGameLoaded(string obj) {
        TimeManager.Instance.OnHourChanged += TimeManager_OnHourChanged;
    }

    private void TimeManager_OnHourChanged() {
        TriggerEvent();
    }

    private void TriggerEvent() {
        if (Random.Range(0, eventChancePerHour) != 0) return;

        Event randomEvent = SelectEventFromWeight();

        // If the event fails to trigger, try another event
        if (!randomEvent.TryTriggerEvent()) {
            Event secondRandomEvent = SelectEventFromWeight();
            secondRandomEvent.TryTriggerEvent();
        }

    }

    private Event SelectEventFromWeight() {
        List<Event> weightedEvents = new List<Event>();
        foreach (Event e in events) {
            for (int i = 0; i < e.eventWeight; i++) {
                weightedEvents.Add(e);
            }
        }

        return weightedEvents[Random.Range(0, weightedEvents.Count)];
    }
}

public enum EventType {
    Neutral,
    Negative,
    Positive,
    Completion
}
