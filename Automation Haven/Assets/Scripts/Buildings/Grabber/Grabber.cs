using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour, ICanBeClicked {

    private PlacedBuildingManager placedBuildingManager;

    private ItemObject grabbedItemObject;
    private ItemSO grabbedItemSO;
    private Transform raycastPoint;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private float timer;
    private float timerMax = 0.8f;

    private Vector3 putDownPosition;
    private PlacedObject_Done putDownObject;
    private Vector3 grabFromPosition;
    private PlacedObject_Done grabPositionObject;
    private PowerReciever powerReciever;
    private List<ItemSO> filteredItems;

    private void Start() {
        filteredItems = new List<ItemSO>();
        raycastPoint = transform.Find("RaycastPoint");
        powerReciever = transform.GetComponent<PowerReciever>();
        placedBuildingManager = PlacedBuildingManager.Instance;
        placedObjectTypeSO = transform.GetComponent<BuildingTypeHolder>().buildingType;
    }


    private void Update() {
        //Vector3 combined = transform.forward + transform.position;
        //Debug.Log("Forward: " + transform.forward + " Transform: " + transform.position + "Combined: " + combined);

        if (!powerReciever.IsPowerAvailable()) { return; }
        GrabObject();
        timer += Time.deltaTime * placedObjectTypeSO.tierMultiplier;
        if (timer > timerMax) {
            timer = 0f;
            PutDownObject();
        }
        
    }

    private void GrabObject() {
        if (grabbedItemObject != null || grabbedItemSO != null)  return;

        grabFromPosition = raycastPoint.position + raycastPoint.forward;
        grabPositionObject = placedBuildingManager.GetPlacedObjectInCell(grabFromPosition);

        if (grabPositionObject == null) return;

        if (grabPositionObject.transform.TryGetComponent(out ICanBeGrabbedFrom canBeGrabbedFrom)) {
            ItemObject potentialgrabbedItemObject = canBeGrabbedFrom.GetPotentialObject();

            if (potentialgrabbedItemObject == null) return;

            if (IsFilteredAndMatchingFilter(potentialgrabbedItemObject.GetItemSO())) {
                grabbedItemObject = potentialgrabbedItemObject;

                canBeGrabbedFrom.GrabObject(grabbedItemObject);
                
                grabbedItemObject.gameObject.SetActive(false);
            }

        }

    }

    /*


    private void HandlePowerProducerGrab() {
        if (!placedBuildingManager.IsNextObjectPowerProducer(grabPositionObject)) return;

        PowerProducer powerProducer = grabPositionObject.transform.GetComponent<PowerProducer>();

        ItemSO itemToTake = powerProducer.GetRequiredItem();

        if (!IsFilteredAndMatchingFilter(itemToTake)) return;

        if (powerProducer.GetStoredAmount() <= 0) return;

        Transform grabbedObjectFromPowerProducer = Instantiate(itemToTake.prefab, grabPositionObject.transform);
        grabbedObjectFromPowerProducer.gameObject.SetActive(false);
        grabbedObject = grabbedObjectFromPowerProducer;
        grabbedItemSO = grabbedObject.GetComponent<ItemObject>().itemSO;

    }*/

    private void PutDownObject() {
        if (grabbedItemObject == null) return;
        putDownPosition = raycastPoint.position + -raycastPoint.forward * 1;
        putDownObject = placedBuildingManager.GetPlacedObjectInCell(putDownPosition);

        if (putDownObject == null) return;

        if (putDownObject.transform.TryGetComponent(out ICanBePutDownIn canBePutDownIn)) {
            if (canBePutDownIn.TryPutDownObject(grabbedItemObject)) {
                ResetGrabbedObject();
            }
        }
    }

    /*

    private void HandlePowerProducerPutDown() {
        if (!placedBuildingManager.IsNextObjectPowerProducer(putDownObject)) return;

        PowerProducer powerProducer = putDownObject.transform.GetComponent<PowerProducer>();
        ItemSO grabbedItemSO = grabbedObject.GetComponent<ItemObject>().itemSO;

        if (powerProducer.GetRequiredItem() != grabbedItemSO || powerProducer.GetStoredAmount() >= powerProducer.GetMaxStorage()) return;

        powerProducer.AddItemToInventory();
        ResetGrabbedObject();
    }

    private void HandleDeliveryPointPutDown() {
        if (!placedBuildingManager.IsNextObjectDeliveryPoint(putDownObject)) return;

        DeliveryPoint deliveryPoint = putDownObject.transform.GetComponent<DeliveryPoint>();
        Debug.Log("Delivery Point Found");
        if (deliveryPoint.IsContractCompleted() || deliveryPoint.GetCurrentContract() == null) return;

        ItemSO grabbedItemSO = grabbedObject.GetComponent<ItemObject>().itemSO;

        //deliveryPoint.DeliverItemsForContract(grabbedItemSO, 1);
        deliveryPoint.DeliverBoxForContract(grabbedObject.GetComponent<Box>());
        ResetGrabbedObject();
    }

    private void HandlePackagerPutDown() {
        if (!placedBuildingManager.IsNextObjectPackager(putDownObject)) return;

        

        BoxPackager boxPackager = putDownObject.transform.GetComponent<BoxPackager>();

        if (boxPackager.IsStorageFull()) return;

        ItemSO grabbedItemSO = grabbedObject.GetComponent<ItemObject>().itemSO;

        bool activeContractNeedsItem = false;

        foreach (ContractManager.Contract contract in ContractManager.Instance.GetActiveContracts()) {
            foreach (ItemAmount itemAmount in contract.neededItemAmount) {
                if (itemAmount.itemSO == grabbedItemSO) {
                    activeContractNeedsItem = true;
                    break;
                }
            }
        }

        if (!activeContractNeedsItem) return;

        boxPackager.AddItemToInventory(grabbedItemSO);
        ResetGrabbedObject();
    }*/

    private void ResetGrabbedObject() {
        grabbedItemSO = null;
        grabbedItemObject = null;
    }

    public void AddNewFilteredItem(ItemSO itemSO) {
        //Debug.Log("Adding item: " + itemSO.nameString);
        powerReciever.AddPowerConsumption(1f);
        filteredItems.Add(itemSO);

    }
    public void RemoveFilteredItem(ItemSO itemSO) {
        //Debug.Log("Removing item: " + itemSO.nameString);
        filteredItems.Remove(itemSO);
        powerReciever.AddPowerConsumption(-1f);
    }

    public List<ItemSO> GetFilteredItems() {
        return filteredItems;
    }
    private bool IsFilteredAndMatchingFilter(ItemSO itemSO) {
        if (filteredItems.Count == 0) return true;
        return filteredItems.Contains(itemSO);
    }

    public void OnClick() {
        SelectFilterUI.Instance.Show(transform);
    }
}
