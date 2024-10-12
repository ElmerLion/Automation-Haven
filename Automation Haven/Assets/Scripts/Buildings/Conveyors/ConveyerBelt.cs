using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConveyerBelt : MonoBehaviour, IItemParent, ICanBeGrabbedFrom, ICanBePutDownIn {

    public event Action<ItemObject> OnItemAdded;
    public event Action<ItemObject> OnItemRemoved;

    [Header("Conveyor Belt Visuals")]
    [SerializeField] private Transform rightTurningBeltVisual;
    [SerializeField] private Transform leftTurningBeltVisual;
    [SerializeField] private Transform straightBeltVisual;
    [SerializeField] private Transform tPointBeltVisual;
    [SerializeField] private Transform crossBeltVisual;

    [Header("Conveyor Belt Settings")]
    [SerializeField] private List<ItemObject> itemsOnBelt;
    [SerializeField] private Transform[] entryPointArray;
    [SerializeField] private Transform middlePoint;
    [SerializeField] private LayerMask itemLayer;

    private PlacedObjectTypeSO placedObjectType;
    private List<ItemObject> removedItemsOnBelt;
    private float speed = 0.8f;
    private float distanceBetweenItems = 0.1f;
    private float distanceToMiddlePoint = 0.01f;
    private float distanceToEntryPoint = 0.2f;
    private float distanceToInputPoint = 0.4f;

    private RaycastHit previousHit;
    private BoxPackager boxPackager;

    private void Awake() {
        itemsOnBelt = new List<ItemObject>();
        removedItemsOnBelt = new List<ItemObject>();
    }

    private void Start() {
        previousHit = default;

        placedObjectType = GetComponent<BuildingTypeHolder>().buildingType;
        speed *= placedObjectType.tierMultiplier;

        GridBuildingSystem.Instance.OnObjectPlaced += GridBuildingSystem_OnObjectPlaced;

        CheckForAdjacentConveyorBelts();
    }


    private void GridBuildingSystem_OnObjectPlaced(object sender, GridBuildingSystem.ObjectPlacedEventArgs e) {
        if (Vector3.Distance(transform.position, e.placedObject.transform.position) <= 2) {
            if (e.placedObject.transform.TryGetComponent(out ConveyerBelt conveyorBelt)) {
                CheckForAdjacentConveyorBelts();
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
    }

    private void Update() {

        foreach (ItemObject item in itemsOnBelt) {
            if (item != null) {
                HandleMovingOnConveyorBelt(item);
            }
        }

        if (removedItemsOnBelt.Count <= 0) return;
        foreach (ItemObject item in removedItemsOnBelt) {
            itemsOnBelt.Remove(item);
            OnItemRemoved?.Invoke(item);
        }
        removedItemsOnBelt.Clear();
    }

    public void HandleMovingOnConveyorBelt(ItemObject itemObject) {
        if (IsObjectInfront(itemObject)) return;

        Vector3 nextPosition = itemObject.transform.position + (itemObject.transform.forward * distanceBetweenItems);
        //Debug.Log("Next position: " + nextPosition);

        PlacedObject_Done nextPlacedObjectDone = PlacedBuildingManager.Instance.GetPlacedObjectInCell(nextPosition);
        if (nextPlacedObjectDone == null) return;

        if (DetectBoxPackagerAtOutput(itemObject) != null) {
            BoxPackager boxPackager = DetectBoxPackagerAtOutput(itemObject);
            if (HasReachedInputPoint(itemObject, boxPackager.transform.Find("InputPoint"))) {
                if (boxPackager.TryAddItemToInputInventory(itemObject)) {
                    removedItemsOnBelt.Add(itemObject);
                }
            }
        }

        if (nextPlacedObjectDone.transform.TryGetComponent(out UndergroundConveyorBelt undergroundConveyorBelt)) {
            if (undergroundConveyorBelt.TryAddItem(itemObject)) {
                removedItemsOnBelt.Add(itemObject);
            }
            return;
        }

        if (!nextPlacedObjectDone.transform.TryGetComponent(out ConveyerBelt nextConveyorBelt)) return;

        MoveItem(itemObject);

        // Adds the item to the next conveyer belt
        if (HasReachedNextConveyorBeltEntryPoint(nextConveyorBelt, itemObject)) {
            nextConveyorBelt.AddItem(itemObject);
            itemObject.hasReachedMiddlePoint = false;
            removedItemsOnBelt.Add(itemObject);
            return;
        }
    }

    private bool HasReachedNextConveyorBeltEntryPoint(ConveyerBelt nextConveyorBelt, ItemObject itemObject) {
        if (nextConveyorBelt == this) return false;

        foreach (Transform entryPoint in nextConveyorBelt.GetEntryPoints()) {
            if (Vector3.Distance(itemObject.transform.position, entryPoint.position) < distanceToEntryPoint) {
                //itemObject.transform.position = entryPoint.position;
                return true;
            }
        }
        return false;
    }

    private void CheckForAdjacentConveyorBelts() {
        bool straightBelt = true;
        bool rightTurningBelt = false;
        bool leftTurningBelt = false;
        bool tPointBelt = false;
        bool crossBelt = false;

        Vector3 rightGridPosition = middlePoint.position + transform.right;
        Vector3 leftGridPosition = middlePoint.position - transform.right;

        // Check adjacent belts
        ConveyerBelt rightConveyor = GetConveyorBeltAtPosition(rightGridPosition);
        ConveyerBelt leftConveyor = GetConveyorBeltAtPosition(leftGridPosition);

        if ((rightConveyor == null && leftConveyor == null) ||  HasStraightConveyorBeltConnected() || IsTPoint(rightConveyor, leftConveyor)) {
            if (IsTPoint(rightConveyor, leftConveyor)) {
                straightBelt = false;
                tPointBelt = true;

            }

            if (IsTPoint(rightConveyor, leftConveyor) && HasStraightConveyorBeltConnected()) {
                tPointBelt = false;
                crossBelt = true;
                straightBelt = false;
            }
        }

        // Handle right visual
        if (rightConveyor != null && rightConveyor.IsFacingDirection(-transform.right) && !tPointBelt && !crossBelt) {
            rightTurningBelt = true;
            straightBelt = false;
        } 

        // Handle left visual
        if (leftConveyor != null && leftConveyor.IsFacingDirection(transform.right) && !tPointBelt && !crossBelt) {
            leftTurningBelt = true;
            straightBelt = false;
        }

        straightBeltVisual.gameObject.SetActive(straightBelt);
        leftTurningBeltVisual.gameObject.SetActive(leftTurningBelt);
        rightTurningBeltVisual.gameObject.SetActive(rightTurningBelt);
        tPointBeltVisual.gameObject.SetActive(tPointBelt);
        crossBeltVisual.gameObject.SetActive(crossBelt);

    }

    private ConveyerBelt GetConveyorBeltAtPosition(Vector3 position) {
        PlacedObject_Done placedObject = PlacedBuildingManager.Instance.GetPlacedObjectInCell(position);
        if (placedObject != null && placedObject.GetComponent<ConveyerBelt>()) {
            return placedObject.GetComponent<ConveyerBelt>();
        }
        return null;
    }

    public bool IsTPoint(ConveyerBelt rightConveyorBelt, ConveyerBelt leftConveyorBelt) {

        bool validRightConveyor = rightConveyorBelt != null && rightConveyorBelt.IsFacingDirection(-transform.right);
        bool validLeftConveyor = leftConveyorBelt != null && leftConveyorBelt.IsFacingDirection(transform.right);

        return validRightConveyor && validLeftConveyor;
    }

    public bool HasStraightConveyorBeltConnected() {
        Vector3 forwardGridPosition = middlePoint.position + transform.forward;
        Vector3 backwardGridPosition = middlePoint.position - transform.forward;

        ConveyerBelt forwardConveyor = GetConveyorBeltAtPosition(forwardGridPosition);
        ConveyerBelt backwardConveyor = GetConveyorBeltAtPosition(backwardGridPosition);

        if (forwardConveyor == null || backwardConveyor == null) {
            return false;
        }

        if (forwardConveyor.transform.rotation != transform.rotation ||  backwardConveyor.transform.rotation != transform.rotation) {
            return false;
        }

        return true;
    }

    public bool IsFacingDirection(Vector3 direction) {
        Vector3 normalizedDirection = direction.normalized;
        float dotProduct = Vector3.Dot(transform.forward, normalizedDirection);
        return dotProduct > 0.9f; 
    }

    private bool HasReachedMiddlePoint(ItemObject itemObject) {
        if (middlePoint == null) return false;

        if (Vector3.Distance(itemObject.transform.position, middlePoint.position) < distanceToMiddlePoint) {
            return true;
        }
        return false;

    }

    private bool HasReachedInputPoint(ItemObject item, Transform inputPoint) {
        return Vector3.Distance(item.transform.position, inputPoint.position) < distanceToInputPoint;
    }

    private BoxPackager DetectBoxPackagerAtOutput(ItemObject itemObject) {
        RaycastHit hit;
        if (Physics.Raycast(itemObject.transform.position, itemObject.transform.forward, out hit, 1f)) {
            if (hit.transform == previousHit.transform) return boxPackager;

            boxPackager = hit.collider.GetComponent<BoxPackager>();
            previousHit = hit;
            return boxPackager;
        }
        return null;
    }

    private bool IsObjectInfront(ItemObject item) {
        bool objectInfront = false;
        Collider[] hitColliders = Physics.OverlapSphere(item.GetRaycastPoint().position, distanceBetweenItems);

        foreach (Collider hitCollider in hitColliders) {
            if (hitCollider.transform != item.transform && hitCollider.GetComponent<ItemObject>() != null) {
                Vector3 directionToCollider = (hitCollider.transform.position - item.transform.position).normalized;
                float dotProduct = Vector3.Dot(item.transform.forward, directionToCollider);


                if (dotProduct > 0.8f) {
                    objectInfront = true;
                    break;
                }
            }
        }
        return objectInfront;
    }

    private void MoveItem(ItemObject itemObject) {
        // ItemObject ska åka fram till mittpunkten, kolla om roteringen är rätt, om inte rotera den rätt och sedan åka framåt på beltet fram tills nästa belt tar över

        if (HasReachedMiddlePoint(itemObject) && !itemObject.hasReachedMiddlePoint) {
             AdjustItemRotation(itemObject.transform, transform.rotation);
            itemObject.hasReachedMiddlePoint = true;
        }

        if (itemObject.hasReachedMiddlePoint) { 
            Vector3 deltaPosition = transform.forward * speed * Time.deltaTime;
            itemObject.transform.position += deltaPosition;
        } else {
            if (middlePoint != null) {
                Vector3 direction = (middlePoint.position - itemObject.transform.position).normalized;
                itemObject.transform.position += direction * speed * Time.deltaTime;
            } else {
                itemObject.transform.position += transform.forward * speed * Time.deltaTime;
            }
        }

    

    }

    private void AdjustItemRotation(Transform item, Quaternion targetRotation) {
        item.rotation = Quaternion.Slerp(item.rotation, targetRotation, Time.deltaTime * 5f);
        item.rotation = targetRotation;
    }

    public Transform[] GetEntryPoints() {
        return entryPointArray;
    }

    public List<ItemObject> GetItems() {
        return itemsOnBelt;
    }

    public void AddItem(ItemObject item, Vector3 spawnPosition = default) {

        itemsOnBelt.Add(item);

        item.SetParent(transform);

        if (spawnPosition != default) {
            item.transform.position = spawnPosition;
            item.transform.rotation = transform.rotation;
        }

        item.gameObject.SetActive(true);

        OnItemAdded?.Invoke(item);
    }

    public void ClearItems() {
        itemsOnBelt.Clear();
    }

    public bool HasItem(ItemObject item) {
        if (itemsOnBelt.Contains(item)) {
            return true;
        }
        return false;
    }

    public bool IsSpaceAvailableOnEntryPoints() {

        foreach (Transform entryPoint in entryPointArray) {
            if (Physics.SphereCast(entryPoint.position, 0.1f, entryPoint.forward, out RaycastHit hit, distanceBetweenItems, itemLayer)) {
                return false;
            }
        }
        return true;
    }

    public float GetDistanceBetweenItems() {
        return distanceBetweenItems;
    }

    public void RemoveItem(ItemObject item) {
        itemsOnBelt.Remove(item);
    }

    public void GrabObject(ItemObject itemObject) {
        RemoveItem(itemObject);
    }

    public ItemObject GetPotentialObject() {
        if (itemsOnBelt.Count <= 0) return null;

        return itemsOnBelt[0];
    }

    public bool TryPutDownObject(ItemObject itemObject) {
        if (itemsOnBelt.Count > 0) return false;

        AddItem(itemObject);

        itemObject.SetParent(transform);
        itemObject.transform.position = entryPointArray[0].position + (transform.forward * distanceBetweenItems);
        itemObject.transform.rotation = transform.rotation;

        itemObject.gameObject.SetActive(true);
        return true;
    }

    private void OnDestroy() {
        GridBuildingSystem.Instance.OnObjectPlaced -= GridBuildingSystem_OnObjectPlaced;
    }
}
