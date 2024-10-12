using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceGeneratorData {
    public float baseSpeedTimerMax;
    public float resourceDetectionRadius;
    public int maxResourceAmount;
    public List<ItemSO> mineableNodes;
}
