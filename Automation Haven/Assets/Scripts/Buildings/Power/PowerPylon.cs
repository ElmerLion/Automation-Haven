using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static PowerManager;

public class PowerPylon : MonoBehaviour {

    [SerializeField] private int networkId;
    [SerializeField] private List<GridObject> gridCellsInRange;

    private int powerRadius = 3;
    private int powerLayer = 8;
    private float totalAvailablePower;

    private List<GameObject> connectedPowerLines;
    [SerializeField] private List<Transform> powerLines;

    private List<PowerReciever> localRecievers;
    private List<PowerProducer> localProducers;
    private List<PowerPylon> localPylons;

    private void Start() {
        PlacedObjectTypeSO placedObjectTypeSO = GetComponent<PlacedObject_Done>().placedObjectTypeSO;
        powerRadius = (int)placedObjectTypeSO.range;

        if (networkId != 0) {
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
        }

        connectedPowerLines = new List<GameObject>();
        localRecievers = new List<PowerReciever>();
        localProducers = new List<PowerProducer>();
        localPylons = new List<PowerPylon>();
        gridCellsInRange = new List<GridObject>();


        Vector3 gridPosition = GridBuildingSystem.Instance.grid.GetWorldPosition((int)transform.position.x, (int)transform.position.z);
        int networkIdOnGrid = IsAnotherNetworkInRange();

        if (networkIdOnGrid != 0) {
            networkId = networkIdOnGrid;
            PowerManager.Instance.AddPowerPylonToNetwork(networkId, this);
            SetNetworkIdInRadius(networkId, powerRadius);
            Debug.Log("Adding to existing network! With networkId " + networkId + " Central pylon at: " + GetCentralPylon().transform.position);
        } else {
            networkId = PowerManager.Instance.CreateNewPowerNetwork(this);
            PowerManager.Instance.AddPowerPylonToNetwork(networkId, this);
            SetNetworkIdInRadius(networkId, powerRadius);
            SetCentralPylon(this);
            Debug.Log("Creating new network! With networkId " + networkId);
        }

        UpdateAvailableBuildings();

        GridBuildingSystem.Instance.OnObjectPlaced += GridBuildingSystem_OnObjectPlaced;
        GridBuildingSystem.Instance.OnObjectDestroyed += GridBuildingSystem_OnObjectDestroyed;
        PowerReciever.OnPowerNeeded += PowerReciever_OnPowerNeeded;
        GetComponent<PlacedObject_Done>().OnBuildingDestroyed += PowerPylon_OnBuildingDestroyed;
        

    }

    private void PowerReciever_OnPowerNeeded(object sender, System.EventArgs e) {
        DistributePower();
    }

    private void GridBuildingSystem_OnObjectDestroyed(object sender, System.EventArgs e) {
        if (GetCentralPylon() != this) {
            return;
        }

        List<PowerProducer> connectedProducers = PowerManager.Instance.GetProducersInNetwork(networkId);
        
        List<PowerProducer> removedProducers = new List<PowerProducer>();
        foreach (PowerProducer producer in connectedProducers) {
            if (producer == null) {
                removedProducers.Add(producer);
            }
        }

        foreach (PowerProducer producer in removedProducers) {
            connectedProducers.Remove(producer);
        }

        List<PowerReciever> connectedRecievers = PowerManager.Instance.GetRecieversInNetwork(networkId);

        List<PowerReciever> removedRecievers = new List<PowerReciever>();
        foreach (PowerReciever reciever in connectedRecievers) {
            if (reciever == null) {
                removedRecievers.Add(reciever);
            }
        }

        foreach (PowerReciever reciever in removedRecievers) {
            connectedRecievers.Remove(reciever);
        }
    }

    private void GridBuildingSystem_OnObjectPlaced(object sender, GridBuildingSystem.ObjectPlacedEventArgs e) {
        UpdateAvailableBuildings();
        
        DistributePower();
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

    public void SetNetworkIdInRadius(int networkId, int radius) {
        bool connectedWithExistingGrid = false;
        int newNetworkId = 0;
        Vector2Int pylonGridPosition = GridBuildingSystem.Instance.GetGridPosition(transform.position);

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
                        newNetworkId = gridObject.GetPowerNetworkId();
                        
                    }
                    
                }
            }
        }

        if (connectedWithExistingGrid) {
            Debug.Log("Merging networks " + newNetworkId + " and " + networkId);
            Instance.MergePowerNetworks(networkId, newNetworkId);
        }

    }

    private void UpdateAvailableBuildings() {
        if (connectedPowerLines == null) {
            connectedPowerLines = new List<GameObject>();
        }

        Collider[] colliders = Physics.OverlapBox(transform.GetChild(0).GetChild(0).position, new Vector3(powerRadius, 1f, powerRadius));
        foreach (Collider collider in colliders) {
            if (collider.gameObject.layer != powerLayer) { continue; }

            PowerReciever powerReciever = collider.transform.GetComponent<PowerReciever>();
            if (powerReciever != null && !Instance.GetRecieversInNetwork(networkId).Contains(powerReciever)) {
                Debug.Log("Adding new reciever");
                Instance.AddRecieverToNetwork(networkId, powerReciever);
                localRecievers.Add(powerReciever);
                continue;
            }

            PowerProducer powerProducer = collider.transform.GetComponent<PowerProducer>();
            if (powerProducer != null && !Instance.GetProducersInNetwork(networkId).Contains(powerProducer)) {
                Debug.Log("Adding new producer");
                Instance.AddProducerToNetwork(networkId, powerProducer);
                localProducers.Add(powerProducer);
                continue;
            }

        }

        Collider[] pylonColliders = Physics.OverlapBox(transform.GetChild(0).GetChild(0).position, new Vector3(powerRadius * 2f, 1f, powerRadius * 2f));
        foreach (Collider collider in pylonColliders) {
            if (collider.gameObject.layer != powerLayer) { continue; }

            PowerPylon powerPylon = collider.transform.GetComponent<PowerPylon>();
            if (powerPylon != null && powerPylon != this && !Instance.GetConnectedPylonsInNetwork(networkId).Contains(powerPylon)) {
                ConnectPowerPylons(powerPylon);
            }
        }

    }

    private void DistributePower() {
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
    }

    private void ConnectPowerPylons(PowerPylon otherPylon) {
        localPylons.Add(otherPylon);
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
            Debug.Log("Other pylon connected to this pylon: " + otherPylon.name + " with power line: " + powerLine.name);
            Debug.Log("To list: " + otherPylon.connectedPowerLines.Count);
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

   

    private void PowerPylon_OnBuildingDestroyed() {

        PowerManager.Instance.RemovePowerPylonFromNetwork(networkId, this);

        if (GetCentralPylon() == this) {
            PowerManager.Instance.AssignNewCentralPylon(networkId);
        }

        PowerManager.Instance.RemoveGridCellsFromNetwork(networkId, gridCellsInRange);

        foreach (PowerProducer producer in localProducers) {
            PowerManager.Instance.RemoveProducerFromNetwork(networkId, producer);
        }

        foreach (PowerReciever reciever in localRecievers) {
            PowerManager.Instance.RemoveRecieverFromNetwork(networkId, reciever);
        }

        RemovePowerLines();
        
        //Instance.CheckAndSplitNetwork(networkId);

        GridBuildingSystem.Instance.OnObjectPlaced -= GridBuildingSystem_OnObjectPlaced;
        GridBuildingSystem.Instance.OnObjectDestroyed -= GridBuildingSystem_OnObjectDestroyed;
        PowerReciever.OnPowerNeeded -= PowerReciever_OnPowerNeeded;
    }

    public int GetNetworkId() {
        return networkId;
    }

    public List<GridObject> GetGridCellsInRange() {
        return gridCellsInRange;
    }
    public PowerPylon GetCentralPylon() {
        return PowerManager.Instance.GetCentralPylon(networkId);
    }
    public void SetNetworkId(int id) {
        networkId = id;
    }
    public void SetCentralPylon(PowerPylon pylon) {
        PowerManager.Instance.SetCentralPylon(networkId, pylon);
    }

    public List<PowerPylon> GetLocalPylons() {
        return localPylons;
    }
}
