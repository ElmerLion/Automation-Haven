using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]  
public class PowerData {

    [Header("Power Data")]
    public float powerProduction;
    public float powerUsage;
    public float powerStorage;
    public float productionRate;

    [Header("Power Production With Resources")]
    public ItemSO requiredItem;
    
}
