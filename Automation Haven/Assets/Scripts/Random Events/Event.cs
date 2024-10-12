using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Event : MonoBehaviour {

    [Header("Event Settings")]
    public EventType eventType;
    public int eventWeight;
    public string eventName; // Display name for the event
    [TextArea(3, 10)]
    public string eventDescription; // Description of the event

    // Initialization method for each event
    public virtual void Initialize(EventType eventType, string eventName, string eventDescription) {
        this.eventType = eventType;
        this.eventName = eventName;
        this.eventDescription = eventDescription;
    }

    // Method to try triggering the event
    public virtual bool TryTriggerEvent() {
        return true;
    }
}
