using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PowerManager;

public class PowerPylon : MonoBehaviour {

    public static event EventHandler<int> OnAnyPowerPylonDestroyed;

    public static void ResetStaticData() {
        OnAnyPowerPylonDestroyed = null;
    }


    [SerializeField] private int networkId;
    private List<GridObject> gridCellsInRange;

    private string uniqueId;
    private int powerRadius = 3;
    private int powerLayer = 8;

    private List<GameObject> connectedPowerLines;
    [SerializeField] private List<Transform> powerLines;

    private List<PowerReciever> localRecievers;
    private List<PowerProducer> localProducers;
    [ES3NonSerializable] [SerializeField] private List<PowerPylon> localPylons;
    private List<string> localPylonIds;
    private List<GameObject> debugObjectsList; 

    private void Start() {
        PlacedObjectTypeSO placedObjectTypeSO = GetComponent<PlacedObject_Done>().placedObjectTypeSO;
        powerRadius = (int)placedObjectTypeSO.range;

        /*if (networkId != 0) {
            Debug.Log("PowerPylon with networkId " + networkId + " loaded from save file.");

            if (connectedPowerLines == null) {
                connectedPowerLines = new List<GameObject>();
            }
            if (localRecievers == null) {
                localRecievers = new List<PowerReciever>();
            }
            if (localProducers == null) {
                localProducers = new List<PowerProducer>();
            }
            if (localPylons == null) {
                localPylons = new List<PowerPylon>();
            }
            if (gridCellsInRange == null) {
                gridCellsInRange = new List<GridObject>();
            }

            UpdateAvailableBuildings();

            GridBuildingSystem.Instance.OnObjectPlaced += GridBuildingSystem_OnObjectPlaced;
            GridBuildingSystem.Instance.OnObjectDestroyed += GridBuildingSystem_OnObjectDestroyed;
            PowerReciever.OnPowerNeeded += PowerReciever_OnPowerNeeded;
            GetComponent<PlacedObject_Done>().OnBuildingDestroyed += PowerPylon_OnBuildingDestroyed;

            SetNetworkIdInRadius(networkId, powerRadius);
            return;
        }*/

        connectedPowerLines = new List<GameObject>();
        localRecievers = new List<PowerReciever>();
        localProducers = new List<PowerProducer>();
        localPylons = new List<PowerPylon>();
        gridCellsInRange = new List<GridObject>();
        debugObjectsList = new List<GameObject>();

        if (localPylonIds == null) {
            localPylonIds = new List<string>();
        }

        foreach (string pylonId in localPylonIds) {
            PowerPylon pylon = PowerManager.Instance.GetPowerPylonById(pylonId);
            localPylons.Add(pylon);
        }


        Vector3 gridPosition = GridBuildingSystem.Instance.grid.GetWorldPosition((int)transform.position.x, (int)transform.position.z);
        int networkIdOnGrid = IsAnotherNetworkInRange();

        if (networkIdOnGrid != 0) {
            networkId = networkIdOnGrid;
            PowerManager.Instance.AddPowerPylonToNetwork(networkId, this);
            SetNetworkIdInRadius(networkId, powerRadius);
        } else {
            networkId = PowerManager.Instance.CreateNewPowerNetwork(out PowerNetwork powerNetwork);
            PowerManager.Instance.AddPowerPylonToNetwork(networkId, this);
            SetNetworkIdInRadius(networkId, powerRadius);
            Debug.Log("Creating new network! With networkId " + networkId);
        }
        
        UpdateAvailableBuildings();

        GridBuildingSystem.Instance.OnObjectPlaced += GridBuildingSystem_OnObjectPlaced;


    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            foreach (GameObject gameObject in debugObjectsList) {
                Destroy(gameObject);
            }
            debugObjectsList.Clear();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.GetChild(0).GetChild(0).position, new Vector3(powerRadius * 2f, 1, powerRadius * 2f));
    }

    public void Save(string uniqueId) {
        this.uniqueId = uniqueId;
        localPylonIds = PowerManager.Instance.GetLocalPylonIds(localPylons);
    }

    private void GridBuildingSystem_OnObjectPlaced(object sender, GridBuildingSystem.ObjectPlacedEventArgs e) {
        if (Vector3.Distance(transform.position, e.placedObject.transform.position) > powerRadius * 2f + 1) { return; }

        UpdateAvailableBuildings();
    }

    public int IsAnotherNetworkInRange() {
        int radius = powerRadius;
        Vector2Int pylonGridPosition = GridBuildingSystem.Instance.GetGridPosition(transform.position);

        for (int x = -radius; x <= radius; x++) {
            for (int z = -radius; z <= radius; z++) {
                Vector2Int gridPosition = new Vector2Int(pylonGridPosition.x + x, pylonGridPosition.y + z);
                GridObject gridObject = GridBuildingSystem.Instance.grid.GetGridObject(gridPosition.x, gridPosition.y);
                if (gridObject != null) {
                    if (gridObject.GetPowerNetworkId() != 0 && gridObject.GetPowerNetworkId() != networkId) {
                        return gridObject.GetPowerNetworkId();
                    }
                }
            }
        }
        return 0;
    }

    private List<GameObject> GetGameObjectsInRadius(int radius) {
        List<GameObject> gameObjects = new List<GameObject>();

        Vector2Int pylonGridPosition = GridBuildingSystem.Instance.GetGridPosition(transform.position);

        for (int x = -radius; x <= radius; x++) {
            for (int z = -radius; z <= radius; z++) {
                Vector2Int gridPosition = new Vector2Int(pylonGridPosition.x + x, pylonGridPosition.y + z);
                PlacedObject_Done placedObject = GridBuildingSystem.Instance.grid.GetGridObject(gridPosition.x, gridPosition.y).GetPlacedObject();

                GameObject debugObject = Instantiate(powerLines[0], new Vector3(gridPosition.x, 0, gridPosition.y), Quaternion.identity).gameObject;
                debugObject.transform.parent = transform;
                Vector3 debugAddPosition = new Vector3(0, 0.5f, 0);
                debugObject.transform.position += debugAddPosition;
                debugObjectsList.Add(debugObject);

                if (placedObject == null) continue;

                GameObject gameObject = placedObject.gameObject;
                gameObjects.Add(gameObject);
            }
        }

        return gameObjects;
    }

    public void SetNetworkIdInRadius(int networkId, int radius) {
        bool connectedWithExistingGrid = false;
        Vector2Int pylonGridPosition = GridBuildingSystem.Instance.GetGridPosition(transform.position);

        List<int> newNetworkIds = new List<int>();

        for (int x = -radius; x <= radius; x++) {
            for (int z = -radius; z <= radius; z++) {
                Vector2Int gridPosition = new Vector2Int(pylonGridPosition.x + x, pylonGridPosition.y + z);
                GridObject gridObject = GridBuildingSystem.Instance.grid.GetGridObject(gridPosition.x, gridPosition.y);
                if (gridObject != null) {
                    PowerManager.Instance.AddGridObjectToNetwork(networkId, gridObject);
                    gridCellsInRange.Add(gridObject);
                    int otherNetworkId = gridObject.GetPowerNetworkId();
                    if (otherNetworkId == 0) {
                        gridObject.SetPowerNetworkId(networkId);

                    }

                    if (otherNetworkId != networkId && otherNetworkId != 0) {
                        connectedWithExistingGrid = true;
                        newNetworkIds.Add(gridObject.GetPowerNetworkId());

                    }

                }
            }
        }

        if (connectedWithExistingGrid) {
            Instance.MergePowerNetworks(networkId, newNetworkIds);
        }

    }

    private void UpdateAvailableBuildings() {
        if (connectedPowerLines == null) {
            connectedPowerLines = new List<GameObject>();
        }

        List<GameObject> availableBuildings = GetGameObjectsInRadius(powerRadius);

        foreach (GameObject availableBuilding in availableBuildings) {
            if (availableBuilding.layer != powerLayer) { continue; }

            PowerReciever powerReciever = availableBuilding.GetComponent<PowerReciever>();
            if (powerReciever != null && !Instance.GetRecieversInNetwork(networkId).Contains(powerReciever)) {
                Instance.AddRecieverToNetwork(networkId, powerReciever);
                localRecievers.Add(powerReciever);
                powerReciever.SetNetworkId(networkId);
                continue;
            }

            PowerProducer powerProducer = availableBuilding.GetComponent<PowerProducer>();
            if (powerProducer != null && !Instance.GetProducersInNetwork(networkId).Contains(powerProducer)) {
                Instance.AddProducerToNetwork(networkId, powerProducer);
                localProducers.Add(powerProducer);
                powerProducer.SetNetworkId(networkId);
                continue;
            }

        }

        availableBuildings = GetGameObjectsInRadius(powerRadius * 2);

        foreach (GameObject availableBuilding in availableBuildings) {
            if (availableBuilding.layer != powerLayer) { continue; }

            PowerPylon powerPylon = availableBuilding.GetComponent<PowerPylon>();
            if (powerPylon != null && powerPylon != this && !localPylons.Contains(powerPylon)) {
                ConnectPowerPylons(powerPylon);
               
            }
        }

    }

    /*private void DistributePower() {
        if (GetCentralPylon() != this) { return; }

        totalAvailablePower = CalculateTotalAvailablePower();
        List<PowerReciever> recieversCopy = new List<PowerReciever>(Instance.GetRecieversInNetwork(networkId));

        foreach (PowerReciever receiver in recieversCopy) {
            //Debug.Log("Trying to distrubute power to " + receiver.name + " with available power: " + totalAvailablePower);
            if (receiver == null) Instance.RemoveRecieverFromNetwork(networkId, receiver);

            if (receiver.IsPowerAvailable()) { continue; }
            float requiredPower = receiver.GetPowerConsumption();
            if (totalAvailablePower >= requiredPower) {
                Debug.Log("Adding power to " + receiver.name);
                float powerToGive = requiredPower;
                receiver.AddAvailablePower(powerToGive);
                DeductPowerFromProducers(powerToGive);
                totalAvailablePower -= powerToGive;
            }

        }
    }

    private float CalculateTotalAvailablePower() {
        float totalPower = 0f;
        foreach (PowerProducer producer in Instance.GetProducersInNetwork(networkId)) {
            totalPower += producer.GetStoredPower();
        }
        return totalPower;
    }

    private void DeductPowerFromProducers(float amount) {
        foreach (PowerProducer producer in Instance.GetProducersInNetwork(networkId)) {
            if (producer.GetStoredPower() >= amount) {
                producer.TakeStoredPower(amount);
                break;
            }

        }
    }*/

    private void ConnectPowerPylons(PowerPylon otherPylon) {
        localPylons.Add(otherPylon);

        if (!otherPylon.localPylons.Contains(this)) {
            otherPylon.localPylons.Add(this);
        }

        if (Instance.GetConnectedPylonsInNetwork(networkId).Contains(otherPylon)) { return; }

        ConnectPowerLinesToPylons(otherPylon);

        Instance.AddConnectedPylonToNetwork(networkId, otherPylon);
    }


    private void ConnectPowerLinesToPylons(PowerPylon otherPylon) {

        Vector3 directionToOtherPylon = otherPylon.transform.position - transform.position;
        float distance = Vector3.Distance(transform.position, otherPylon.transform.position);

        if (powerLines.Count > 0) {
            Transform powerPoleVisualTransform = transform.GetChild(0).GetChild(0);
            Vector3 spawnPoint = (powerPoleVisualTransform.position + new Vector3(0, 5.5f, 0));
            Quaternion rotation = Quaternion.LookRotation(directionToOtherPylon);

            GameObject powerLine = Instantiate(powerLines[0].gameObject, spawnPoint, rotation);
            powerLine.transform.parent = transform;

            Vector3 newScale = powerLine.transform.localScale;
            newScale.x = distance * 0.125f;
            powerLine.transform.localScale = newScale;

            PlacedObject_Done thisPylonPlacedObject = GetComponent<PlacedObject_Done>();
            PlacedObject_Done otherPylonPlacedObject = otherPylon.GetComponent<PlacedObject_Done>();

            // Calculate rotation adjustment based on the direction of each pylon
            float rotationAdjustment = thisPylonPlacedObject.placedObjectTypeSO.GetRotationAngle(thisPylonPlacedObject.dir) -
                                       otherPylonPlacedObject.placedObjectTypeSO.GetRotationAngle(otherPylonPlacedObject.dir);
            powerLine.transform.Rotate(new Vector3(0, -90, 0));

            connectedPowerLines.Add(powerLine);

            if (otherPylon.connectedPowerLines == null) {
                otherPylon.connectedPowerLines = new List<GameObject>();
            }
            otherPylon.connectedPowerLines.Add(powerLine);

        }
    }

    private void RemovePowerLines() {
        foreach (GameObject powerLine in connectedPowerLines) {
            if (powerLine != null) {
                Destroy(powerLine);
            }
        }
        connectedPowerLines.Clear();
    }

    private void OnDestroy() {
        RemovePowerLines();

        GridBuildingSystem.Instance.OnObjectPlaced -= GridBuildingSystem_OnObjectPlaced;
        
        OnAnyPowerPylonDestroyed?.Invoke(this, networkId);
    }

    public int GetNetworkId() {
        return networkId;
    }

    public List<GridObject> GetGridCellsInRange() {
        return gridCellsInRange;
    }

    public List<PowerProducer> GetLocalProducers() {
        return localProducers;
    }

    public List<PowerReciever> GetLocalRecievers() {
        return localRecievers;
    }

    public void SetNetworkId(int id) {
        networkId = id;
    }

    public List<PowerPylon> GetLocalPylons() {
        return localPylons;
    }

    public string GetUniqueId() {
        return uniqueId;
    }
}
