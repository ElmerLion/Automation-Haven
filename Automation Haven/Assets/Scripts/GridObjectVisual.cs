using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridObjectVisual : MonoBehaviour {

    [SerializeField] private TextMeshPro xzText;
    [SerializeField] private TextMeshPro networkIdText;

    private GridObject gridObject;

    public void Setup(GridObject gridObject) {
        this.gridObject = gridObject;
        gridObject.OnNetworkIDChanged += GridObject_OnNetworkIDChanged;

        SetXZText(gridObject.x + ", " + gridObject.y);
        SetNetworkIdText(gridObject.powerNetworkId.ToString());
    }


    private void GridObject_OnNetworkIDChanged(object sender, int e) {
        SetNetworkIdText(e.ToString());
    }

    public void SetXZText(string text) {
        xzText.text = text;
    }

    public void SetNetworkIdText(string text) {
        networkIdText.text = text;
        networkIdText.ForceMeshUpdate();
    }
    
}
