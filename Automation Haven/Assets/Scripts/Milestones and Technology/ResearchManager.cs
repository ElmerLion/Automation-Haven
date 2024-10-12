using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResearchManager;

public class ResearchManager : MonoBehaviour {

    public static ResearchManager Instance { get; private set; }

    public enum ResearchCategory {
        Factory,
        Automation,
        Products,
        Energy,
    }

    public event EventHandler<ResearchNode> OnActiveResearchProgressChanged;
    public event EventHandler<ResearchNode> OnNodeResearched;
    public event EventHandler OnResearchQueueChanged;
    public event EventHandler OnResearchLoaded;

    [SerializeField] private List<ResearchCategorySections> researchCategorySectionList;

    [SerializeField] private List<ResearchNode> allResearchNodeList;
    [SerializeField] private List<ResearchNodeSO> researchedNodeSOList;
    [SerializeField] private List<ResearchNode> researchQueue;
    private int maxResearchQueueSize = 3;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SaveManager.OnGameSaved += SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded += SaveManager_OnGameLoaded;
    }

    private void SaveManager_OnGameLoaded(string obj) {
        LoadResearch(obj);
    }

    private void SaveManager_OnGameSaved(string obj) {
        SaveResearch(obj);
    }

    private void SetupResearchNodes() {

        foreach (ResearchCategorySections researchCategorySection in researchCategorySectionList) {
            foreach (ResearchSection researchSection in researchCategorySection.researchSections) {
                foreach (ResearchNodeSO researchNodeSO in researchSection.researchSectionSO.researchNodes) {
                    if (GetResearchNode(researchNodeSO) != null) continue;
                    ResearchNode researchNode = new ResearchNode(researchNodeSO);

                    allResearchNodeList.Add(researchNode);
                }
            }
        }
    }
    private void CheckForResearchedNodes() {
        List<ResearchNodeSO> researchedNodeSOListCopy = new List<ResearchNodeSO>(researchedNodeSOList);

        foreach (ResearchNodeSO researchNodeSO in researchedNodeSOListCopy) {
            SetResearchAsCompleted(GetResearchNode(researchNodeSO));
        }
    }

    public bool TryAddNewResearchToQueue(ResearchNode researchNode) {
        if (researchNode.isResearched) return false;
        if (researchQueue.Count >= maxResearchQueueSize) return false;
        if (researchQueue.Contains(researchNode)) return false;

        foreach (ResearchNodeSO prerequisiteResearchSlot in researchNode.researchNodeSO.prerequisiteResearchList) {
            if (!researchedNodeSOList.Contains(prerequisiteResearchSlot)) return false;
        }

        Debug.Log("AllResearchNodeList Contains: " + allResearchNodeList.Contains(researchNode));
        Debug.Log("AllResearchNodeList Count: " + allResearchNodeList.Count);
        if (allResearchNodeList.Contains(researchNode)) {
            researchQueue.Add(researchNode);
            OnResearchQueueChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log("Adding to Queue");
            return true;
        }

        return false;
    }

    public void StopResearchNode(ResearchNode researchNode) {
        if (researchQueue.Contains(researchNode)) {
            researchQueue.Remove(researchNode);
            OnResearchQueueChanged?.Invoke(this, EventArgs.Empty);
            Debug.Log("Research removed from queue: " + researchNode.researchNodeSO.name);
        }
    }

    private void ProgressResearchQueue() {
        if (researchQueue.Count <= 0) return;

        ResearchNode currentResearchNode = researchQueue[0];
        currentResearchNode.ProgressResearch();

        OnActiveResearchProgressChanged?.Invoke(this, currentResearchNode);

        if (currentResearchNode.researchProgress >= currentResearchNode.researchNodeSO.researchCost) {
            CompleteResearch(currentResearchNode);
        }
    }

    private void SetResearchAsCompleted(ResearchNode researchNode) {
        researchNode.Complete();
        if (!researchedNodeSOList.Contains(researchNode.researchNodeSO)) {
            researchedNodeSOList.Add(researchNode.researchNodeSO);
        }

        if (researchQueue.Contains(researchNode)) {
            researchQueue.Remove(researchNode);
            OnResearchQueueChanged?.Invoke(this, EventArgs.Empty);
        }

        UnlockResearchNodeObjects(researchNode);

        OnNodeResearched?.Invoke(this, researchNode);

    }

    private void CompleteResearch(ResearchNode researchNode) {
        researchNode.Complete();
        researchedNodeSOList.Add(researchNode.researchNodeSO);
        researchQueue.RemoveAt(0);

        OnResearchQueueChanged?.Invoke(this, EventArgs.Empty);

        UnlockResearchNodeObjects(researchNode);

        MessageBarUI.Instance.CreateMessage("Research Completed", researchNode.researchNodeSO.nameString + " has been researched", EventType.Completion);

        OnNodeResearched?.Invoke(this, researchNode);
    }

    private void UnlockResearchNodeObjects(ResearchNode researchNode) {
        foreach (PlacedObjectTypeSO unlockedBuilding in researchNode.researchNodeSO.unlockedBuildings) {
            PlacedBuildingManager.Instance.UnlockBuilding(unlockedBuilding);
        }

        foreach (RecipeSO unlockedRecipe in researchNode.researchNodeSO.unlockedRecipes) {
            ItemManager.Instance.UnlockItemAndRecipe(unlockedRecipe);
        }
    }

    public ResearchNode GetResearchNode(ResearchNodeSO researchNodeSO) {
        foreach (ResearchNode researchNode in allResearchNodeList) {
            if (researchNode.researchNodeSO == researchNodeSO) return researchNode;
        }

        return null;
    }

    public bool DoesResearchQueueContainNode(ResearchNode researchNode) {
        return researchQueue.Contains(researchNode);
    }

    public List<ResearchSection> GetResearchSectionList() {
        List<ResearchSection> researchSections = new List<ResearchSection>();
        foreach (ResearchCategorySections researchCategorySections in researchCategorySectionList) {
            researchSections.AddRange(researchCategorySections.researchSections);
        }

        return researchSections;
    }

    public int GetResearchQueueCount() {
        return researchQueue.Count;
    }

    public List<ResearchNode> GetResearchQueue() {
        return new List<ResearchNode>(researchQueue);
    }

    public ResearchNode GetActiveResearchNode() {
        return researchQueue.Count > 0 ? researchQueue[0] : null;
    }

    public List<ResearchSection> GetResearchSectionsInCategory(ResearchCategory category) {
        List<ResearchSection> researchSections = new List<ResearchSection>();

        foreach (ResearchCategorySections researchCategorySection in researchCategorySectionList) {
            if (researchCategorySection.researchCategory == category) {
                researchSections.AddRange(researchCategorySection.researchSections);
            }
        }

        return researchSections;
    }

    public void SaveResearch(string saveFilePath) {
        List<ResearchNodeSO> researchQueueSOList = new List<ResearchNodeSO>();
        foreach (ResearchNode researchNode in researchQueue) {
            researchQueueSOList.Add(researchNode.researchNodeSO);
        }

        ES3.Save("researchQueueSOList", researchQueueSOList, saveFilePath);
        ES3.Save("researchedNodeSOList", researchedNodeSOList, saveFilePath);
        ES3.Save("allResearchNodeList", allResearchNodeList, saveFilePath);
    }

    public void LoadResearch(string saveFilePath) {
        researchedNodeSOList = ES3.Load("researchedNodeSOList", saveFilePath, new List<ResearchNodeSO>());
        allResearchNodeList = ES3.Load("allResearchNodeList", saveFilePath, new List<ResearchNode>());
        List<ResearchNodeSO> researchQueueSOList = ES3.Load("researchQueueSOList", saveFilePath, new List<ResearchNodeSO>());

        SetupResearchNodes();

        foreach (ResearchNodeSO researchNodeSO in researchQueueSOList) {
            ResearchNode researchNode = GetResearchNode(researchNodeSO);
            if (researchNode != null) {
                researchQueue.Add(researchNode);
            }
        }

        OnResearchLoaded?.Invoke(this, EventArgs.Empty);

        CheckForResearchedNodes();
        InvokeRepeating(nameof(ProgressResearchQueue), 0f, 1f);

    }

    [System.Serializable]
    public class ResearchNode {
        public event EventHandler OnResearchedStatusChanged;
        public event EventHandler OnResearchProgressChanged;

       
        public ResearchNodeSO researchNodeSO;
        public bool isResearched;
        public int researchProgress;

        public ResearchNode(ResearchNodeSO researchNodeSO) {
            this.researchNodeSO = researchNodeSO;
            isResearched = false;
            researchProgress = 0;
        }

        public void ProgressResearch() {
            researchProgress++;
            OnResearchProgressChanged?.Invoke(this, EventArgs.Empty);

            Debug.Log("Research Progress: " + researchProgress + "/" + researchNodeSO.researchCost);
        }

        public void Complete() {
            isResearched = true;
            researchProgress = researchNodeSO.researchCost;
            OnResearchedStatusChanged?.Invoke(this, EventArgs.Empty);

            Debug.Log("Research Completed: " + researchNodeSO.name);
        }

        public float GetResearchProgressNormalized() {
            return (float)researchProgress / researchNodeSO.researchCost;
        }
    }

    [Serializable]
    public class ResearchSection {
        [HideInInspector] public Transform transform;

        public ResearchSectionSO researchSectionSO;
    }

    [Serializable]
    public class ResearchCategorySections {
        public ResearchCategory researchCategory;
        public List<ResearchSection> researchSections;
    }

    private void OnDestroy() {
        SaveManager.OnGameSaved -= SaveManager_OnGameSaved;
        SaveManager.OnGameLoaded -= SaveManager_OnGameLoaded;
    }
}
