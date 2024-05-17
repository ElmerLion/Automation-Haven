using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTypeHolder : MonoBehaviour, IShowWorldTooltip {

    public PlacedObjectTypeSO buildingType;

    public string GetTooltipInfo() {
        string tooltipText = buildingType.nameString;

        return tooltipText;
    }
}
