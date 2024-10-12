using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGhost : MonoBehaviour {

    [SerializeField] private Transform inputArrow;
    [SerializeField] private Transform outputArrow;
    [SerializeField] private Transform rangeVisual;

    private Transform visual;
    private PlacedObjectTypeSO placedObjectTypeSO;

    private void Start() {
        RefreshVisual();
        rangeVisual.gameObject.SetActive(false);
        inputArrow.gameObject.SetActive(false);
        outputArrow.gameObject.SetActive(false);

        GridBuildingSystem.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e) {
        RefreshVisual();
    }

    private void LateUpdate() {
        Vector3 targetPosition = GridBuildingSystem.Instance.GetMouseWorldSnappedPosition();
        targetPosition.y = 1f;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);

        transform.rotation = Quaternion.Lerp(transform.rotation, GridBuildingSystem.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);
    }

    private void RefreshVisual() {
        if (visual != null) {
            Destroy(visual.gameObject);
            rangeVisual.gameObject.SetActive(false);
            inputArrow.gameObject.SetActive(false);
            outputArrow.gameObject.SetActive(false);
            visual = null;
        }

        PlacedObjectTypeSO placedObjectTypeSO = GridBuildingSystem.Instance.GetPlacedObjectTypeSO();

        if (placedObjectTypeSO != null) {
            visual = Instantiate(placedObjectTypeSO.visual, Vector3.zero, Quaternion.identity);
            visual.parent = transform;
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;

            if (placedObjectTypeSO.hasInput) {
                inputArrow.gameObject.SetActive(true);
                Transform inputPoint = placedObjectTypeSO.prefab.Find("InputPoint");
                Vector3 inputPosition = inputPoint.position + placedObjectTypeSO.prefab.position + visual.position;
                Vector3 nextGridPosition = inputPosition + inputPoint.forward * 0.8f;
                Vector3 offsetPosition = new Vector3(nextGridPosition.x, visual.position.y + 0.5f, nextGridPosition.z);

                inputArrow.position = offsetPosition;

                Vector3 inputRotation = inputPoint.eulerAngles;
                inputRotation.y += 180;
                inputArrow.rotation = Quaternion.Euler(inputRotation);
            } else {
                inputArrow.gameObject.SetActive(false);
            }

            if (placedObjectTypeSO.hasOutput) {
                outputArrow.gameObject.SetActive(true);
                Transform outputPoint = placedObjectTypeSO.prefab.Find("OutputPoint");
                Vector3 outputPosition = outputPoint.position + placedObjectTypeSO.prefab.position + visual.position;
                Vector3 nextGridPosition = outputPosition + outputPoint.forward * 0.8f;
                Vector3 offsetPosition = new Vector3(nextGridPosition.x, visual.position.y + 0.5f, nextGridPosition.z);
                outputArrow.rotation = outputPoint.rotation;
                outputArrow.position = offsetPosition;
            } else {
                outputArrow.gameObject.SetActive(false);
            }

            if (placedObjectTypeSO.range > 0) {
                rangeVisual.gameObject.SetActive(true);
                rangeVisual.localScale = new Vector3(placedObjectTypeSO.range * 2, 1f, placedObjectTypeSO.range * 2);
            } else {
                rangeVisual.gameObject.SetActive(false);
            }

            SetLayerRecursive(visual.gameObject, 6);
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer) {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform) {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

}
