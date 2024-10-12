using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNodeGenerator : MonoBehaviour {

    public static ResourceNodeGenerator Instance { get; private set; }

    [Header("Resource Node Generation")]
    [SerializeField] private Transform resourceNodePrefab;
    [SerializeField] private Transform liquidNodePrefab;
    [SerializeField] private GameObject clusterContainer;
    [SerializeField] private Transform resourceNodesCluster;

    [Header("Resource Node Generation Settings")]
    [SerializeField] private int distanceToBorder = 3;
    [SerializeField] private List<ResourceNodeSettings> possibleResourceNodeItems;

    private HashSet<Vector3> occupiedNodePositions;
    private List<ResourceNodeData> resourceNodeDataList;

    private int amountOfNodesPerCluster;
    private int gridWidth;
    private int gridHeight;


    private void Start() {
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
    }

    private void GenerateResourceNodes() {

        foreach (ResourceNodeSettings resourceNodeSettings in possibleResourceNodeItems) {
            int amountOfClusters = (gridWidth * gridHeight) / 30000 * resourceNodeSettings.amountOfClustersPer30000Cells;

            for (int i = 0; i < amountOfClusters; i++) {
                Vector3 middlePos = new Vector3(Random.Range(0, gridWidth), 0, Random.Range(0, gridHeight));

                GenerateResourceCluster(middlePos, resourceNodeSettings);
            }
        }

        
    }

    private void GenerateResourceCluster(Vector3 middlePos, ResourceNodeSettings resourceNodeSettings) {
        amountOfNodesPerCluster = Random.Range(resourceNodeSettings.nodesPerClusterMin, resourceNodeSettings.nodesPerClusterMax) + 1;
        Transform cluster = Instantiate(resourceNodesCluster, clusterContainer.transform);

        cluster.position = middlePos;
        int attempts = 0;
        float maxDistanceFromCenter = (resourceNodeSettings.nodeSpreadDistance + 1);
        int currentResourceNodesToNextMaxDistance = 0;
        float nextMaxDistanceMultiplier = 1;

        for (int j = 0; j < amountOfNodesPerCluster; j++) {
            if (currentResourceNodesToNextMaxDistance > 5 * nextMaxDistanceMultiplier) {
                maxDistanceFromCenter += (resourceNodeSettings.nodeSpreadDistance + 1);
                currentResourceNodesToNextMaxDistance = 0;
                nextMaxDistanceMultiplier += 0.7f;
            }

            Vector3 spawnPositionAddon = Vector3.zero;
            bool validPositionFound = false;
            int validPositionAttempts = 0;

            while (!validPositionFound && validPositionAttempts < 100) {
                spawnPositionAddon = new Vector3(Mathf.RoundToInt(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter)), 0, Mathf.RoundToInt(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter)));
                Vector3 potentialSpawnPosition = middlePos + spawnPositionAddon;

                // Check if this position is a valid spot (not too close to other nodes)
                validPositionFound = true;
                foreach (Vector3 occupiedPosition in occupiedNodePositions) {
                    if (Vector3.Distance(potentialSpawnPosition, occupiedPosition) < resourceNodeSettings.nodeSpreadDistance) {
                        validPositionFound = false;
                        break;
                    }
                }
                validPositionAttempts++;
            }

            if (!validPositionFound) {
                continue;
            }

            //Vector3 spreadDistanceAddon = new Vector3(Random.Range(-resourceNodeSettings.nodeSpreadDistance, resourceNodeSettings.nodeSpreadDistance), 0, Random.Range(-resourceNodeSettings.nodeSpreadDistance, resourceNodeSettings.nodeSpreadDistance));
            //Vector3 spawnPositionAddon = new Vector3(Mathf.RoundToInt(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter)), 0, Mathf.RoundToInt(Random.Range(-maxDistanceFromCenter, maxDistanceFromCenter))) + spreadDistanceAddon;
            Vector3 resourceSpawnPosition = middlePos + spawnPositionAddon;

            if (resourceSpawnPosition.x <= 0) resourceSpawnPosition.x = 0 + distanceToBorder;
            if (resourceSpawnPosition.x >= gridWidth) resourceSpawnPosition.x = gridWidth - distanceToBorder;
            if (resourceSpawnPosition.z <= 0) resourceSpawnPosition.z = 0 + distanceToBorder;
            if (resourceSpawnPosition.z >= gridHeight) resourceSpawnPosition.z = gridHeight - distanceToBorder;


            if (occupiedNodePositions.Contains(resourceSpawnPosition)) {
                attempts++;
                if (attempts > 100) continue;
                j--;

                continue;
            }

            Transform resourceNode = Instantiate(resourceNodeSettings.resourceItemSO.resourceNodePrefab, cluster).transform;
            

            resourceNode.position = resourceSpawnPosition;
            GridBuildingSystem.Instance.grid.GetGridObject(resourceSpawnPosition).SetPlacedObject(resourceNode.GetComponent<PlacedObject_Done>());

            occupiedNodePositions.Add(resourceSpawnPosition);
            resourceNode.GetComponent<ResourceNode>().resourceItemNode = resourceNodeSettings.resourceItemSO;
            currentResourceNodesToNextMaxDistance += resourceNodeSettings.nodeSpreadDistance + 1;

            resourceNodeDataList.Add(new ResourceNodeData(resourceSpawnPosition, resourceNodeSettings.resourceItemSO));
        }

    }

    
    private void SaveManager_OnGameSaved(string obj) {
        ES3.Save("resourceNodeDataList", resourceNodeDataList, obj);
    }

    private void SaveManager_OnGameLoaded(string obj) {
        possibleResourceNodeItems = NewWorldManager.Instance.GetResourceNodeSettings();
        resourceNodeDataList = ES3.Load("resourceNodeDataList", obj, new List<ResourceNodeData>());

        if (resourceNodeDataList == null) resourceNodeDataList = new List<ResourceNodeData>();

        foreach (ResourceNodeData resourceNodeData in resourceNodeDataList) {
            GameObject resourceNode = Instantiate(resourceNodeData.resourceItemSO.resourceNodePrefab, resourceNodeData.position, Quaternion.identity);

            GridBuildingSystem.Instance.grid.GetGridObject(resourceNodeData.position).SetPlacedObject(resourceNode.GetComponent<PlacedObject_Done>());
        }

        if (resourceNodeDataList.Count > 0) return;

        gridWidth = GridBuildingSystem.Instance.grid.GetWidth();
        gridHeight = GridBuildingSystem.Instance.grid.GetHeight();

        occupiedNodePositions = new HashSet<Vector3>();
        resourceNodeDataList = new List<ResourceNodeData>();

        GenerateResourceNodes();
        occupiedNodePositions.Clear();

    }

    public void SetResourceNodeSettings(List<ResourceNodeSettings> settings) {
        possibleResourceNodeItems = settings;
    }



    public class ResourceNodeData {
        public Vector3 position;
        public ItemSO resourceItemSO;

        public ResourceNodeData(Vector3 position, ItemSO resourceItemSO) {
            this.position = position;
            this.resourceItemSO = resourceItemSO;
        }
    }

    [System.Serializable]
    public class ResourceNodeSettings {
        public ItemSO resourceItemSO;
        public int amountOfClustersPer30000Cells;
        public int nodesPerClusterMax;
        public int nodesPerClusterMin;
        public int nodesPerClusterDifference;
        public int nodeSpreadDistance;
    }

}
