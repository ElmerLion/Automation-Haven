using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PowerManager : MonoBehaviour {
    
    public static PowerManager Instance { get; private set; }

    private const string powerNetworksKey = "powerNetworks";
    private const string currentNetworkIdKey = "currentNetworkId";

    [SerializeField] private List<PowerNetwork> powerNetworks;

    private Dictionary<string, PowerPylon> powerPylonIds;
    private int currentNetworkId;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    private void PowerReciever_OnPowerNeeded(object sender, EventArgs e) {
        PowerReciever reciever = sender as PowerReciever;
        if (reciever == null) return;

        if (reciever.IsPowerAvailable()) return;

        DistributePower(reciever.GetNetworkId(), reciever);
    }

    private void PowerPylon_OnAnyPowerPylonDestroyed(object sender, int networkId) {
        PowerPylon powerPylon = sender as PowerPylon;

        RemoveGridCellsFromNetwork(networkId, powerPylon.GetGridCellsInRange());

        foreach (PowerProducer producer in powerPylon.GetLocalProducers()) {
            RemoveProducerFromNetwork(networkId, producer);
        }

        foreach (PowerReciever reciever in powerPylon.GetLocalRecievers()) {
            RemoveRecieverFromNetwork(networkId, reciever);
        }

        foreach (PowerPylon neighbourPylon in powerPylon.GetLocalPylons()) {
            neighbourPylon.GetLocalPylons().Remove(powerPylon);
        }

        RemovePowerPylonFromNetwork(networkId, powerPylon);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        powerNetworks = ES3.Load(powerNetworksKey, obj, new List<PowerNetwork>());
        Debug.Log("Power networks loaded: " + powerNetworks.Count);

        currentNetworkId = ES3.Load(currentNetworkIdKey, obj, 1);
        powerPylonIds = ES3.Load("powerPylonIds", obj, new Dictionary<string, PowerPylon>());
        Debug.Log("Current network id loaded: " + currentNetworkId);

        PowerPylon.OnAnyPowerPylonDestroyed += PowerPylon_OnAnyPowerPylonDestroyed;
        PowerReciever.OnPowerNeeded += PowerReciever_OnPowerNeeded;
        GridBuildingSystem.Instance.OnBeforeObjectDestroyed += GridBuildingSystem_OnBeforeObjectDestroyed;
    }

    private void GridBuildingSystem_OnBeforeObjectDestroyed(object sender, GridBuildingSystem.ObjectDestroyedEventArgs e) {
        if (e.placedObject.transform.TryGetComponent(out PowerReciever powerReciever)) {
            int networkId = powerReciever.GetNetworkId();
            if (networkId == 0) return;

            PowerNetwork powerNetwork = GetPowerNetwork(networkId);
            if (powerNetwork == null) return;

            RemoveRecieverFromNetwork(powerNetwork.networkId, powerReciever);

            foreach (PowerPylon pylon in powerNetwork.powerPylons) {
                if (pylon.GetLocalRecievers().Contains(powerReciever)) {
                    pylon.GetLocalRecievers().Remove(powerReciever);
                }
            }
        }
        if (e.placedObject.transform.TryGetComponent(out PowerProducer powerProducer)) {
            int networkId = powerProducer.GetNetworkId();
            if (networkId == 0) return;

            PowerNetwork powerNetwork = GetPowerNetwork(networkId);
            if (powerNetwork == null) return;

            if (powerNetwork != null) {
                RemoveProducerFromNetwork(powerNetwork.networkId, powerProducer);

                foreach (PowerPylon pylon in powerNetwork.powerPylons) {
                    if (pylon.GetLocalProducers().Contains(powerProducer)) {
                        pylon.GetLocalProducers().Remove(powerProducer);
                    }
                }
            }
        }
    }

    private void SaveManager_OnGameSaved(string obj) {
        if (powerNetworks.Count == 0) {
            return;
        }

        ES3.Save(powerNetworksKey, powerNetworks, obj);

        ES3.Save(currentNetworkIdKey, currentNetworkId, obj);

        foreach (PowerNetwork network in powerNetworks) {
            foreach (PowerPylon pylon in network.powerPylons) {
                pylon.Save(GenerateUniqueId());
            }
        }

        ES3.Save("powerPylonIds", powerPylonIds, obj);

        Debug.Log("Power networks saved: " + powerNetworks.Count);
    }

    public string GenerateUniqueId() {
        return  Guid.NewGuid().ToString();
    }

    public PowerPylon GetPowerPylonById(string id) {
        if (powerPylonIds.ContainsKey(id)) {
            return powerPylonIds[id];
        }
        return null;
    }

    public List<string> GetLocalPylonIds(List<PowerPylon> pylons) {
        List<string> pylonIds = new List<string>();
        foreach (PowerPylon pylon in pylons) {
            pylonIds.Add(pylon.GetUniqueId());
        }
        return pylonIds;
    }

    public int GetNextNetworkId() {
        currentNetworkId = powerNetworks.Count + 1;
        return currentNetworkId;
    }

    public int CreateNewPowerNetwork(out PowerNetwork powerNetwork) {
        int networkId = GetNextNetworkId();
        powerNetwork = new PowerNetwork(networkId);
        powerNetworks.Add(powerNetwork);
        return networkId;
    }

    public void AddPowerPylonToNetwork(int networkId, PowerPylon pylon) {
        PowerNetwork network = powerNetworks.FirstOrDefault(n => n.networkId == networkId);
        if (network != null && !network.powerPylons.Contains(pylon)) {
            network.powerPylons.Add(pylon);
        }
    }

    public void RemovePowerPylonFromNetwork(int networkId, PowerPylon pylon) {
        PowerNetwork network = GetPowerNetwork(networkId);

        if (!network.powerPylons.Contains(pylon)) return;

        if (network != null) {
            network.powerPylons.Remove(pylon);
            if (network.powerPylons.Count == 0) {
                RemoveNetwork(networkId);
            } else {
                CheckAndSplitNetwork(network.networkId);
            }
        }
    }

    public void AddGridObjectToNetwork(int networkId, GridObject gridObject) {
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                network.gridCellsInNetwork.Add(gridObject);
                return;
            }
        }
    }

    public PowerNetwork GetPowerNetwork(int networkId) {
        //Debug.Log("Power networks: " + powerNetworks.Count + " Looking for " + networkId);
        foreach (PowerNetwork network in powerNetworks) {
            //Debug.Log("Checking network " + network.networkId + " for " + networkId);
            if (network.networkId == networkId) {
                //Debug.Log("Found network");
                return network;
            }
        }
        return null;
    }

    // This method needs to make sure it doesnt remove grid cells that might be connected to other pylons
    public void RemoveGridCellsFromNetwork(int networkId, List<GridObject> gridObjects) {
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                foreach (GridObject gridObject in gridObjects) {
                    if (network.gridCellsInNetwork.Contains(gridObject)) {
                        gridObject.SetPowerNetworkId(0);
                        network.gridCellsInNetwork.Remove(gridObject);
                    }

                }
                return;
            }
        }
    }

    public void MergePowerNetworks(int networkIdToUse, List<int> otherNetworkIds) {
        Debug.Log("Merging power networks...");
        if (otherNetworkIds.Contains(networkIdToUse)) {
            otherNetworkIds.Remove(networkIdToUse);
        }

        if (otherNetworkIds.Count == 0) return;

        PowerNetwork network = GetPowerNetwork(networkIdToUse);
        List<PowerNetwork> otherNetworks = new List<PowerNetwork>();

        foreach (int id in otherNetworkIds) {
            PowerNetwork otherNetwork = GetPowerNetwork(id);
            if (otherNetwork != null) {
                otherNetworks.Add(otherNetwork);
            }
        }

        if (otherNetworks.Count == 0) return;

        foreach (PowerNetwork otherNetwork in otherNetworks) {
            MergeNetworks(network, otherNetwork);
        }
    }

    private void MergeNetworks(PowerNetwork network, PowerNetwork otherNetwork) {
        if (network == null || otherNetwork == null) return;

        int networkIdToUse = network.networkId;

        foreach (PowerPylon pylon in otherNetwork.powerPylons) {
            if (network.powerPylons.Contains(pylon)) continue;
            pylon.SetNetworkId(networkIdToUse);
            network.powerPylons.Add(pylon);
        }

        foreach (GridObject gridObject in otherNetwork.gridCellsInNetwork) {
            if (network.gridCellsInNetwork.Contains(gridObject)) continue;
            network.gridCellsInNetwork.Add(gridObject);
            gridObject.SetPowerNetworkId(networkIdToUse);
        }

        List<PowerReciever> recievers = GetRecieversInNetwork(networkIdToUse);
        List<PowerReciever> otherNeworkRecievers = GetRecieversInNetwork(otherNetwork.networkId);
        if (recievers.Count > 0 && otherNeworkRecievers != null) {
            foreach (PowerReciever reciever in otherNeworkRecievers) {
                if (!recievers.Contains(reciever)) {
                    recievers.Add(reciever);
                }
            }
        }
        List<PowerProducer> producers = GetProducersInNetwork(networkIdToUse);
        List<PowerProducer> otherNetworkProducers = GetProducersInNetwork(otherNetwork.networkId);
        if (producers.Count > 0 && otherNetworkProducers != null) {
            foreach (PowerProducer producer in otherNetworkProducers) {
                if (!producers.Contains(producer)) {
                    producers.Add(producer);
                }
            }
        }

        network.networkId = networkIdToUse;

        powerNetworks.Remove(otherNetwork);
    }

    private void CheckAndSplitNetwork(int networkId) {
        PowerNetwork originalNetwork = GetPowerNetwork(networkId);
        if (originalNetwork == null) return;

        List<PowerPylon> allPylons = new List<PowerPylon>(originalNetwork.powerPylons); // Copy to avoid modification issues during iteration.

        if (allPylons.Count == 0) {
            RemoveNetwork(networkId);
            return;
        }

        // Use a Dictionary to keep track of which pylon is in which new network.
        Dictionary<PowerPylon, HashSet<PowerPylon>> newNetworks = new Dictionary<PowerPylon, HashSet<PowerPylon>>();

        while (allPylons.Count > 0) {
            if (allPylons[0] == null) {
                allPylons.RemoveAt(0);
                continue;
            }

            HashSet<PowerPylon> visitedPylons = new HashSet<PowerPylon>();
            ExploreNetwork(allPylons[0], ref visitedPylons);

            // Create or add to new network grouping.
            newNetworks[visitedPylons.First()] = visitedPylons;

            allPylons.RemoveAll(p => visitedPylons.Contains(p));
        }

        // If only one group exists and it contains all pylons, no split is needed.
        if (newNetworks.Count == 1 && newNetworks.First().Value.Count == originalNetwork.powerPylons.Count) {
            return;
        }

        // Remove original network as it's now split.
        RemoveNetwork(networkId);

        // Create new networks based on the groups found.
        foreach (KeyValuePair<PowerPylon, HashSet<PowerPylon>> networkSet in newNetworks) {
            CreateAndPopulateNewNetwork(networkSet.Value);
        }
    }

    private void ExploreNetwork(PowerPylon startPylon, ref HashSet<PowerPylon> visitedPylons) {
        Queue<PowerPylon> queue = new Queue<PowerPylon>();
        queue.Enqueue(startPylon);
        while (queue.Count > 0) {
            PowerPylon currentPylon = queue.Dequeue();
            if (currentPylon == null) continue;
            if (!visitedPylons.Contains(currentPylon)) {
                visitedPylons.Add(currentPylon);
                foreach (PowerPylon neighbor in currentPylon.GetLocalPylons()) {
                    if (!visitedPylons.Contains(neighbor)) {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    private void CreateAndPopulateNewNetwork(HashSet<PowerPylon> pylons) {
        if (pylons.Count == 0) return;

        CreateNewPowerNetwork(out PowerNetwork newNetwork);

        UpdateNetworkPylons(newNetwork, pylons);
    }


    private void RemoveNetwork(int networkId) {
        powerNetworks.Remove(GetPowerNetwork(networkId));
    }

    private void UpdateNetworkPylons(PowerNetwork newNetwork, HashSet<PowerPylon> pylons) {
        int newNetworkId = newNetwork.networkId;

        foreach (PowerPylon pylon in pylons) {
            pylon.SetNetworkId(newNetworkId);

            newNetwork.AddPylon(pylon);

            newNetwork.AddProducerList(pylon.GetLocalProducers());
            newNetwork.AddRecieverList(pylon.GetLocalRecievers());


            foreach (GridObject gridObject in pylon.GetGridCellsInRange()) {
                AddGridObjectToNetwork(newNetworkId, gridObject);
                gridObject.SetPowerNetworkId(newNetworkId);
            }
        }
    }

    public void DistributePower(int networkId, PowerReciever targetReciever) {
        PowerNetwork network = GetPowerNetwork(networkId);

        if (network == null) return;

        List<PowerProducer> producers = network.powerProducers;
        List<PowerReciever> recievers = network.powerRecievers;

        if (producers.Count == 0 || recievers.Count == 0) return;

        float totalPower = 0;
        foreach (PowerProducer producer in producers) {
            totalPower += producer.GetStoredPower();
        }

        float neededPower = targetReciever.GetPowerConsumption();

        if (totalPower < neededPower) {
            Debug.Log("Not enough power");
            return;
        }

        float powerToDistribute = neededPower;

        foreach (PowerProducer producer in producers) {
            if (powerToDistribute <= 0) break;

            float powerToTake = Mathf.Min(powerToDistribute, producer.GetStoredPower());
            producer.TakeStoredPower(powerToTake);
            targetReciever.AddAvailablePower(powerToTake);
            powerToDistribute -= powerToTake;
        }
    }


    public List<PowerProducer> GetProducersInNetwork(int networkId) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            return network.powerProducers;
        }
        return null;
    }

    public List<PowerReciever> GetRecieversInNetwork(int networkId) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            return network.powerRecievers;
        }
        return null;
    }

    public List<PowerPylon> GetConnectedPylonsInNetwork(int networkId) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            return network.powerPylons;
        }
        return null;
    }

    public void AddProducerToNetwork(int networkId, PowerProducer producer) {
        PowerNetwork network = GetPowerNetwork(networkId);

        if (network != null) {
            if (network.powerProducers.Contains(producer)) return;

            network.powerProducers.Add(producer);
        }
    }

    public void AddRecieverToNetwork(int networkId, PowerReciever reciever) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            if (network.powerRecievers.Contains(reciever)) return;

            network.powerRecievers.Add(reciever);
        }
    }

    public void AddConnectedPylonToNetwork(int networkId, PowerPylon pylon) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            network.powerPylons.Add(pylon);
        }
    }

    public void RemoveProducerFromNetwork(int networkId, PowerProducer producer) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            network.powerProducers.Remove(producer);
        }
    }

    public void RemoveRecieverFromNetwork(int networkId, PowerReciever reciever) {
        PowerNetwork network = GetPowerNetwork(networkId);
        if (network != null) {
            network.powerRecievers.Remove(reciever);
        }
    }

    [System.Serializable]
    public class PowerNetwork {
        public int networkId;

        public List<PowerPylon> powerPylons;
        public List<GridObject> gridCellsInNetwork;

        public List<PowerProducer> powerProducers;
        public List<PowerReciever> powerRecievers;

        public PowerNetwork(int networkId) {
            this.networkId = networkId;
            powerPylons = new List<PowerPylon>();
            gridCellsInNetwork = new List<GridObject>();
            powerProducers = new List<PowerProducer>();
            powerRecievers = new List<PowerReciever>();
        }

        public void SetProducers(List<PowerProducer> producers) {
            powerProducers = producers;
        }

        public void SetRecievers(List<PowerReciever> recievers) {
            powerRecievers = recievers;
        }

        public List<PowerPylon> GetPowerPylons() {
            return powerPylons;
        }

        public void AddPylon(PowerPylon pylon) {
            powerPylons.Add(pylon);
        }

        public void RemovePylon(PowerPylon pylon) {
            powerPylons.Remove(pylon);
        }

        public void AddProducerList(List<PowerProducer> producers) {
            powerProducers.AddRange(producers);
        }

        public void AddRecieverList(List<PowerReciever> recievers) {
            powerRecievers.AddRange(recievers);
        }

        public void RemoveProducerList(List<PowerProducer> producers) {
            foreach (PowerProducer producer in producers) {
                powerProducers.Remove(producer);
            }
        }

        public void RemoveRecieverList(List<PowerReciever> recievers) {
            foreach (PowerReciever reciever in recievers) {
                powerRecievers.Remove(reciever);
            }
        }
    }
}
