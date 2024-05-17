using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : MonoBehaviour, IHasProgress {

    private ResourceGeneratorData resourceGeneratorData;
    public static Dictionary<ItemSO, int> GetNearbyResourceNodeAmount(Transform sender, ResourceGeneratorData resourceGeneratorData) {
        Collider[] colliderArray = Physics.OverlapBox(sender.GetChild(0).GetChild(0).position, new Vector3(resourceGeneratorData.resourceDetectionRadius, 1f, resourceGeneratorData.resourceDetectionRadius));
        Dictionary<ItemSO, int> itemSOResourceNodeAmountDic = new Dictionary<ItemSO, int>();

        //int nearbyResourceNodeAmount = 0;
        foreach (Collider collider in colliderArray) {
            ResourceNode resourceNode = collider.GetComponent<ResourceNode>();

            Debug.Log("Collider: " + collider.name);
            
            if (resourceNode == null) { continue; }

            if (itemSOResourceNodeAmountDic.ContainsKey(resourceNode.resourceItemNode)) {
                itemSOResourceNodeAmountDic[resourceNode.resourceItemNode]++;
            } else {
                itemSOResourceNodeAmountDic.Add(resourceNode.resourceItemNode, 1);
            }
        }

        //nearbyResourceNodeAmount = Mathf.Clamp(nearbyResourceNodeAmount, 0, resourceGeneratorData.maxResourceAmount);

        return itemSOResourceNodeAmountDic;
    }

    public event EventHandler OnNoResourceNodesNearby;

    private Dictionary<ItemSO, int> itemSOResourceNodeAmountDic;
    private PowerReciever powerReciever;
    private Transform raycastPoint;
    private float timer;
    private float productionSpeed;
    private List<ItemSO> mineableNodes;
    private PlacedBuildingManager placedBuildingManager;
    private int totalResourceNodes;
    private bool enableResourceGeneration = true;

    public event Action OnProgressChanged;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.GetChild(0).GetChild(0).position, new Vector3(resourceGeneratorData.resourceDetectionRadius, 1f, resourceGeneratorData.resourceDetectionRadius));
    }

    private void Awake() {
        Debug.Log("Awake");

        resourceGeneratorData = transform.GetComponent<BuildingTypeHolder>().buildingType.resourceGeneratorData;
        raycastPoint = transform.Find("OutputPoint");
        powerReciever = transform.GetComponent<PowerReciever>();
    }

    private void Start() {
        Debug.Log("Start");

        enableResourceGeneration = true;

        if (itemSOResourceNodeAmountDic != null && itemSOResourceNodeAmountDic.Count > 0) {
            Debug.Log("Existing Data Found");
            return;
        }

        placedBuildingManager = PlacedBuildingManager.Instance;
        mineableNodes = resourceGeneratorData.mineableNodes;
        itemSOResourceNodeAmountDic = new Dictionary<ItemSO, int>();

        itemSOResourceNodeAmountDic = GetNearbyResourceNodeAmount(transform, resourceGeneratorData);

        if (itemSOResourceNodeAmountDic.Count <= 0 ) {
            OnNoResourceNodesNearby?.Invoke(this, EventArgs.Empty);
            enableResourceGeneration = false;
        }
        bool isValidResourceNode = false;
        List<ItemSO> resourceNodesToRemove = new List<ItemSO>();
        foreach (ItemSO itemSO in itemSOResourceNodeAmountDic.Keys) {
            if (!mineableNodes.Contains(itemSO)) {
                resourceNodesToRemove.Add(itemSO);
            }
            if (mineableNodes.Contains(itemSO)) {
                isValidResourceNode = true;
            }
        }

        if (isValidResourceNode == false) { enabled = false; }

        foreach (ItemSO itemSO in resourceNodesToRemove) {
            itemSOResourceNodeAmountDic.Remove(itemSO);
        }

        totalResourceNodes = 0;
        foreach (ItemSO itemSO in itemSOResourceNodeAmountDic.Keys) {
            Debug.Log("Item: " + itemSO + " Amount: " + itemSOResourceNodeAmountDic[itemSO]);
            totalResourceNodes += itemSOResourceNodeAmountDic[itemSO];
        }

        productionSpeed = (resourceGeneratorData.baseSpeedTimerMax / 2f) + resourceGeneratorData.baseSpeedTimerMax * (1 - (float)totalResourceNodes / resourceGeneratorData.maxResourceAmount);

    }

    private void Update() {
        if (!enableResourceGeneration) { return; }
        if (productionSpeed <= 0) { return; }
        if (!powerReciever.IsPowerAvailable()) { return; }

        HandleResourceGenerationAndOutput();
    }

    private void HandleResourceGenerationAndOutput() {
        timer += Time.deltaTime;
        if (timer <= productionSpeed) {
            OnProgressChanged?.Invoke();
        }

        if (timer >= productionSpeed) {

            Vector3 nextGridPosition = raycastPoint.position + raycastPoint.forward;
            PlacedObject_Done nextGridObject = PlacedBuildingManager.Instance.GetPlacedObjectInCell(nextGridPosition);

            if (placedBuildingManager.IsNextObjectConveyorBelt(nextGridObject)) {
                ConveyerBelt putDownConveyerBelt = nextGridObject.transform.GetComponent<ConveyerBelt>();

                if (putDownConveyerBelt.GetItems().Count < 2) {
                    Vector3 spawnPosition = putDownConveyerBelt.GetEntryPoints()[0].position + (nextGridObject.transform.forward / 2);

                    ItemSO itemToSpawn = SelectItemBasedOnWeight();

                    Transform generatedItem = Instantiate(itemToSpawn.prefab, spawnPosition, nextGridObject.transform.rotation);
                    putDownConveyerBelt.AddItem(generatedItem.GetComponent<ItemObject>());

                    timer = 0;
                    OnProgressChanged?.Invoke();
                    powerReciever.ConsumePower();
                }
            }

            HandleStorageOutput(nextGridObject);
            
        }
    }

    private void HandleStorageOutput(PlacedObject_Done nextGridObject) {
        if (placedBuildingManager.IsNextObjectStorage(nextGridObject)) {
            Storage outputStorage = nextGridObject.transform.GetComponent<Storage>();
            Inventory outputStorageInventory = outputStorage.GetInventory();
            if (outputStorageInventory == null || outputStorage == null) return;

            ItemSO itemToSpawn = SelectItemBasedOnWeight();

            if (outputStorageInventory.IsMaxInventorySlotsReached() && outputStorageInventory.GetInventorySlotWithSpaceLeft(itemToSpawn, 1) == null) return;

            Transform generatedItem = Instantiate(itemToSpawn.prefab, outputStorage.transform);
            ItemObject itemObject = generatedItem.GetComponent<ItemObject>();

            if (outputStorage.TryAddItemObjectToInventory(itemObject)) {
                timer = 0;
                OnProgressChanged?.Invoke();
                powerReciever.ConsumePower();
            }
        }
    }
     
    private ItemSO SelectItemBasedOnWeight() {
        int totalWeight = 0;
        foreach (var kvp in itemSOResourceNodeAmountDic) {
            totalWeight += kvp.Value;
        }

        int randomNumber = UnityEngine.Random.Range(0, totalWeight);
        foreach (var kvp in itemSOResourceNodeAmountDic) {
            if (randomNumber < kvp.Value) {
                return kvp.Key;
            }
            randomNumber -= kvp.Value;
        }

        throw new InvalidOperationException("Unable to select item; check item weights");
    }

    public float GetProgressNormalized() {
        return timer / productionSpeed;
    }

    public float GetMaxProgress() {
        return resourceGeneratorData.baseSpeedTimerMax;
    }

    private void OnEnable() {
        Debug.Log("ResourceGenerator is enabled");
    }

    private void OnDisable() {
        Debug.Log("ResourceGenerator is disabled");
    }
}
