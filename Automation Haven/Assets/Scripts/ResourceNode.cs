using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceNode : MonoBehaviour, IShowWorldTooltip {

    public ItemSO resourceItemNode;

    public string GetTooltipInfo() {
        string tooltipText = resourceItemNode.nameString;
        if (resourceItemNode.chemicalSymbol != "") {
            tooltipText += " ( " + resourceItemNode.chemicalSymbol + " )";
        }

        return tooltipText;
    }
}
