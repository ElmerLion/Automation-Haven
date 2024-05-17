using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingClickedHandler : MonoBehaviour {

    private void OnMouseDown() {
        if (GridBuildingSystem.Instance.GetPlacedObjectTypeSO() != null) {
            return;
        }

        if (transform.TryGetComponent(out ICanBeClicked canBeClicked) && !IsMouseOverUI()) {
            canBeClicked.OnClick();
        }
       
    }

    private bool IsMouseOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
    }

}
