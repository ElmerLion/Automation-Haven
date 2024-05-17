using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerManager : MonoBehaviour {
    
    public static PowerManager Instance { get; private set; }

    private const string powerNetworksKey = "powerNetworks";
    private const string currentNetworkIdKey = "currentNetworkId";

    [SerializeField] private List<PowerNetwork> powerNetworks = new List<PowerNetwork>();

    private int currentNetworkId;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadPowerNetworks();
    }

    public int GetNextNetworkId() {
        currentNetworkId = powerNetworks.Count + 1;
        return currentNetworkId;
    }

    public int CreateNewPowerNetwork(PowerPylon pylon) {
        int networkId = GetNextNetworkId();
        PowerNetwork newNetwork = new PowerNetwork(pylon, networkId);
        powerNetworks.Add(newNetwork);
        return networkId;
    }

    public void AssignNewCentralPylon(int networkId) {
        List<PowerReciever> recievers = GetRecieversInNetwork(networkId);
        List<PowerProducer> producers = GetProducersInNetwork(networkId);

        foreach (PowerNetwork network in powerNetworks) {
            
            if (network.networkId == networkId) {
                if (network.powerPylons.Count == 0) {
                    Debug.LogWarning("No pylons in network");
                    RemoveNetwork(networkId);
                    return;
                }
                network.centralPylon = network.powerPylons[0];
                if (recievers.Count > 0) {
                    network.SetRecievers(recievers);
                }
                if (producers.Count > 0) {
                    network.SetProducers(producers);
                }
                
                

                foreach (PowerPylon pylon in network.powerPylons) {
                    pylon.SetCentralPylon(network.centralPylon);
                }
                return;
            }
        }
    }

    public void AddPowerPylonToNetwork(int networkId, PowerPylon pylon) {
        PowerNetwork network = powerNetworks.FirstOrDefault(n => n.networkId == networkId);
        if (network != null && !network.powerPylons.Contains(pylon)) {
            network.powerPylons.Add(pylon);
        }
    }

    public void RemovePowerPylonFromNetwork(int networkId, PowerPylon pylon) {
        List<PowerNetwork> powerNetworksCopy = new List<PowerNetwork>(powerNetworks);
        foreach (PowerNetwork powerNetwork in powerNetworksCopy) {
            if (powerNetwork.networkId == networkId) {
                if (powerNetwork != null) {
                    if (powerNetwork.powerPylons.Contains(pylon)) {
                        powerNetwork.powerPylons.Remove(pylon);
                    }

                    if (!powerNetwork.powerPylons.Any()) {
                        powerNetworks.Remove(powerNetwork);
                    }
                }
            }
        }
    }

    public PowerPylon GetCentralPylon(int networkId) {
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                return network.centralPylon;
            }
        }
        return null;
    }

    public void SetCentralPylon(int networkId, PowerPylon pylon) {
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                network.centralPylon = pylon;
                return;
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
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                return network;
            }
        }
        return null;
    }

    public void RemoveGridCellsFromNetwork(int networkId, List<GridObject> gridObjects) {
        foreach (PowerNetwork network in powerNetworks) {
            if (network.networkId == networkId) {
                foreach (GridObject gridObject in gridObjects) {
                    gridObject.SetPowerNetworkId(0);
                    network.gridCellsInNetwork.Remove(gridObject);
                }
                return;
            }
        }
    }

    public void MergePowerNetworks(int networkIdToUse, int otherNetworkId) {
        Debug.Log("Merging power networks...");
        if (networkIdToUse == otherNetworkId) {
            Debug.LogError("Cannot merge the same network");
            return;
        }

        PowerNetwork network = GetPowerNetwork(networkIdToUse);
        PowerNetwork otherNetwork = GetPowerNetwork(otherNetworkId);


        if (network == null) {
            Debug.LogError("Connecting network is null");
            return;
        }
        if (otherNetwork == null) {
            Debug.LogError("Other network is null");
            return;
        }
        
        foreach (PowerPylon pylon in otherNetwork.powerPylons) {
            Debug.Log(pylon.transform.position);
            pylon.SetNetworkId(networkIdToUse);
            network.powerPylons.Add(pylon);
        }

        foreach (GridObject gridObject in otherNetwork.gridCellsInNetwork) {
            network.gridCellsInNetwork.Add(gridObject);
            gridObject.SetPowerNetworkId(networkIdToUse);
        }

        List<PowerReciever> recievers = GetRecieversInNetwork(networkIdToUse); 
        recievers.AddRange(GetRecieversInNetwork(otherNetworkId));
        List<PowerProducer> producers = GetProducersInNetwork(networkIdToUse);
        producers.AddRange(GetProducersInNetwork(otherNetworkId));

        AssignNewCentralPylon(networkIdToUse);
        network.networkId = networkIdToUse;

        powerNetworks.Remove(otherNetwork);
        //otherNetwork = null;
    }

    public void CheckAndSplitNetwork(int networkId) {
        List<PowerPylon> connectedPylons = GetConnectedPylonsInNetwork(networkId);

        if (connectedPylons.Count == 0) {
            RemoveNetwork(networkId);
            return;
        }

        List<PowerPylon> visitedPylons = new List<PowerPylon>();
        ExploreNetwork(connectedPylons[0], ref visitedPylons);

        // Only split if not all pylons were visited
        if (visitedPylons.Count < connectedPylons.Count) {
            SplitNetwork(visitedPylons, networkId);
        }
    }

    private void ExploreNetwork(PowerPylon startPylon, ref List<PowerPylon> visitedPylons) {
        var stack = new Stack<PowerPylon>();
        stack.Push(startPylon);

        while (stack.Count > 0) {
            var currentPylon = stack.Pop();
            if (!visitedPylons.Contains(currentPylon)) {
                visitedPylons.Add(currentPylon);
                foreach (var connectedPylon in currentPylon.GetLocalPylons()) {
                    if (!visitedPylons.Contains(connectedPylon)) {
                        stack.Push(connectedPylon);
                    }
                }
            }
        }
    }

    private void SplitNetwork(List<PowerPylon> visitedPylons, int originalNetworkId) {
        List<PowerPylon> allPylons = GetConnectedPylonsInNetwork(originalNetworkId);
        List<PowerPylon> unvisitedPylons = allPylons.Except(visitedPylons).ToList();

        // Existing network retains its central pylon
        if (visitedPylons.Contains(GetCentralPylon(originalNetworkId))) {
            UpdateNetworkPylons(visitedPylons, originalNetworkId, GetCentralPylon(originalNetworkId));
        } else {
            PowerPylon newCentralPylonForExistingNetwork = visitedPylons[0];
            int newNetworkIdForRemaining = CreateNewPowerNetwork(newCentralPylonForExistingNetwork);
            UpdateNetworkPylons(visitedPylons, newNetworkIdForRemaining, newCentralPylonForExistingNetwork);
        }

        // Create new networks from unvisited pylons
        while (unvisitedPylons.Count > 0) {
            List<PowerPylon> newNetworkPylons = new List<PowerPylon>();
            ExploreNetwork(unvisitedPylons[0], ref newNetworkPylons);

            PowerPylon newCentralPylon = newNetworkPylons[0];
            int newNetworkId = CreateNewPowerNetwork(newCentralPylon);
            UpdateNetworkPylons(newNetworkPylons, newNetworkId, newCentralPylon);

            unvisitedPylons.RemoveAll(pylon => newNetworkPylons.Contains(pylon));
        }
    }

    private void RemoveNetwork(int networkId) {
        Debug.Log("Removing network " + networkId);
        powerNetworks.Remove(GetPowerNetwork(networkId));
    }

    private void UpdateNetworkPylons(List<PowerPylon> pylons, int newNetworkId, PowerPylon newCentralPylon) {
        foreach (PowerPylon pylon in pylons) {
            RemovePowerPylonFromNetwork(pylon.GetNetworkId(), pylon);
            pylon.SetNetworkId(newNetworkId);
            pylon.SetCentralPylon(newCentralPylon);
            AddPowerPylonToNetwork(newNetworkId, pylon);
        }
    }

    private void SavePowerNetworks() {
        //ES3.Save(powerNetworksKey, powerNetworks);
        if (powerNetworks.Count == 0) {
            return;
        }

        ES3.Save(powerNetworksKey, powerNetworks);

        ES3.Save(currentNetworkIdKey, currentNetworkId);
        

        Debug.Log("Power networks saved: " + powerNetworks.Count);
    }

    private void LoadPowerNetworks() {
        if (ES3.KeyExists(powerNetworksKey)) {
            powerNetworks = ES3.Load<List<PowerNetwork>>(powerNetworksKey);
            Debug.Log("Power networks loaded: " + powerNetworks.Count);
        } else {
            powerNetworks = new List<PowerNetwork>();
        }

        currentNetworkId = ES3.Load(currentNetworkIdKey, 1);
        Debug.Log("Current network id loaded: " + currentNetworkId);
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

    private void OnApplicationQuit() {
        SavePowerNetworks();
    }

    [System.Serializable]
    public class PowerNetwork {
        public PowerPylon centralPylon;
        public int networkId;

        public List<PowerPylon> powerPylons;
        public List<GridObject> gridCellsInNetwork;

        public List<PowerProducer> powerProducers;
        public List<PowerReciever> powerRecievers;

        public PowerNetwork(PowerPylon centralPylon, int networkId) {
            this.centralPylon = centralPylon;
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
    }

}
